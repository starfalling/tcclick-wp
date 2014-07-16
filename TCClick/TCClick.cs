using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Net.NetworkInformation;
using System.Net.Sockets;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Linq;


namespace com.truecolor.wp {



  public class TCClick {

    private static TCClick sharedInstance;
    private string uploadUrl;
    private string channel;
    private TCClickDataContext db;
    private int lastActivityStartAt = 0;
    private int maxActivityId = 0;
    private int maxEventId = 0;

    public static TCClick SharedInstance() {
      if (sharedInstance == null) {
        sharedInstance = new TCClick();
      }
      return sharedInstance;
    }

    public TCClick() : base() {
      InitDatabase();
    }

    private void InitDatabase() {
      db = new TCClickDataContext();
      if (db.DatabaseExists() == false) {
        db.CreateDatabase();
      }
      lastActivityStartAt = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    private void UploadData() {
      byte[] data = buildDataToUpload();
      HttpWebRequest request = WebRequest.Create(uploadUrl) as HttpWebRequest;
      request.Method = "POST";
      request.BeginGetRequestStream(new AsyncCallback(r => {
        using (Stream s = request.EndGetRequestStream(r)) {
          s.Write(data, 0, data.Length);
        }


        try {
          request.BeginGetResponse(new AsyncCallback(r2 => {
            HttpWebResponse response = request.EndGetResponse(r2) as HttpWebResponse;
            if (response.StatusCode == HttpStatusCode.OK) {
              // upload completed, delete the data in local database that uploaded
              if (maxActivityId > 0) {
                var sql = from m in db.DbModelActivities where m.Id <= maxActivityId select m;
                db.DbModelActivities.DeleteAllOnSubmit(sql);
              }
              if (maxEventId > 0) {
                var sql = from m in db.DbModelEvents where m.Id <= maxEventId select m;
                db.DbModelEvents.DeleteAllOnSubmit(sql);
              }

              db.SubmitChanges();
            }
          }), request);
        } catch (Exception e) {}
      }), request);

    }

    private byte[] buildDataToUpload() {
      Dictionary<String, Object> data = new Dictionary<String, Object>();
      data["timestamp"] = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      data["device"] = Device.getDeviceJsonMetrics(channel);

      List<JObject> activities = new List<JObject>();
      var dbItems = from DbModelActivity activity in db.DbModelActivities select activity;
      foreach (DbModelActivity activity in dbItems) {
        JObject item = new JObject();
        item["start_at"] = activity.StartAt;
        item["end_at"] = activity.EndAt;
        if (maxActivityId < activity.Id) {
          maxActivityId = activity.Id;
        }
        activities.Add(item);
      }

      List<JObject> events = new List<JObject>();
      var dbItems2 = from DbModelEvent activity in db.DbModelEvents select activity;
      foreach (DbModelEvent e in dbItems2) {
        JObject item = new JObject();
        item["name"] = e.Name;
        item["param"] = e.Param;
        item["value"] = e.Value;
        item["version"] = e.Version;
        item["created_at"] = e.CreatedAt;
        if (maxEventId < e.Id) {
          maxEventId = e.Id;
        }
        events.Add(item);
      }

      Dictionary<String, Object> d = new Dictionary<String, Object>();
      d["activities"] = activities;
      d["events"] = events;
      data["data"] = d;

      byte[] bytes = new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(data));
      System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(data));
      //System.Diagnostics.Debug.WriteLine(Decompress(Compress(bytes)));
      return Compress(bytes);
    }

    private Byte[] Compress(Byte[] bytes) {
      using (var memoryStream = new MemoryStream()) {
        using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
          deflateStream.Write(bytes, 0, bytes.Length);
        return memoryStream.ToArray();
      }
    }
    private static string Decompress(Byte[] bytes) {
      using (var uncompressed = new MemoryStream())
      using (var compressed = new MemoryStream(bytes))
      using (var ds = new DeflateStream(compressed, CompressionMode.Decompress)) {
        ds.CopyTo(uncompressed);
        byte[] b = uncompressed.ToArray();
        return Encoding.UTF8.GetString(b, 0, b.Length);
      }
    }

    public static void Init(string uploadUrl, string channel) {
      TCClick.SharedInstance().uploadUrl = uploadUrl;
      TCClick.SharedInstance().channel = channel;
      TCClick.SharedInstance().UploadData();
    }

    private void insertActivity(int startAt, int endAt) {
      if (startAt == 0) return;
      DbModelActivity m = new DbModelActivity { StartAt = startAt , EndAt = endAt};
      db.DbModelActivities.InsertOnSubmit(m);
      db.SubmitChanges();
    }

    public void Application_Activated(object sender, Microsoft.Phone.Shell.ActivatedEventArgs e) {
      System.Diagnostics.Debug.WriteLine("Application_Activated");
      lastActivityStartAt = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      UploadData();
    }
    public void Application_Deactivated(object sender, Microsoft.Phone.Shell.DeactivatedEventArgs e) {
      System.Diagnostics.Debug.WriteLine("Application_Deactivated");
      insertActivity(lastActivityStartAt, (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
      lastActivityStartAt = 0;
    }
    public void Application_Closing(object sender, Microsoft.Phone.Shell.ClosingEventArgs e) {
      System.Diagnostics.Debug.WriteLine("Application_Closing");
      insertActivity(lastActivityStartAt, (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
      lastActivityStartAt = 0;
    }

    private void _Event(string name, string param, string value) {
      int timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      DbModelEvent m = new DbModelEvent { Name = name, Param = param, Value=value, CreatedAt = timestamp,
      Version=Device.GetAppVersion()};
      db.DbModelEvents.InsertOnSubmit(m);
      db.SubmitChanges();
    }

    public static void Event(string name) {
      Event(name, name);
    }
    public static void Event(string name, string value) {
      Event(name, name, value);
    }

    /// <summary>
    /// log that an event happen
    /// </summary>
    /// <param name="name">the name of the event</param>
    /// <param name="param">an event can have many params and values, you may think this of subevent</param>
    /// <param name="value"></param>
    public static void Event(string name, string param, string value) {
      TCClick.SharedInstance()._Event(name, param, value);
    }

  }






  internal sealed class Device {
    internal static JObject getDeviceJsonMetrics(String channel) {
      if (channel == null || channel.Equals("")) channel = "WindowsAppStore";
      JObject data = new JObject();
      data["udid"] = GetUdid();
      data["app_version"] = GetAppVersion();
      data["channel"] = channel;
      com.truecolor.wp.Ailon.WP.Utils.CanonicalPhoneName n = com.truecolor.wp.Ailon.WP.Utils.PhoneNameResolver.Resolve(
        Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer,
        Microsoft.Phone.Info.DeviceStatus.DeviceName);
      data["brand"] = n.CanonicalManufacturer;
      data["model"] = n.CanonicalModel;
      data["os_version"] = System.Environment.OSVersion.Version.Major + "." + System.Environment.OSVersion.Version.Minor;
      data["carrier"] = Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.CellularMobileOperator;
      string width = Application.Current.Host.Content.ActualWidth.ToString();
      string height = Application.Current.Host.Content.ActualHeight.ToString();
      data["resolution"] = width + "×" + height;
      data["local"] = System.Globalization.CultureInfo.CurrentCulture.Name;
      if (DeviceNetworkInformation.IsNetworkAvailable) {
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        s.Bind(new IPEndPoint(IPAddress.Any, 10000));
        NetworkInterfaceInfo info = s.GetCurrentNetworkInterface();
        data["network"] = info.InterfaceType.ToString();
        s.Close();
        s.Dispose();
      }
      return data;
    }


    internal static string GetUdid() {
      return BitConverter.ToString(
            (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId")
            ).Replace("-", "");
    }

    internal static string appVersion = null;
    internal static string GetAppVersion() {
      if (appVersion == null) {
        var xmlReaderSettings = new System.Xml.XmlReaderSettings {
          XmlResolver = new System.Xml.XmlXapResolver()
        };
        using (var xmlReader = System.Xml.XmlReader.Create("WMAppManifest.xml", xmlReaderSettings)) {
          xmlReader.ReadToDescendant("App");
          appVersion = xmlReader.GetAttribute("Version");
        }
      }
      return appVersion;
    }
  }

}
