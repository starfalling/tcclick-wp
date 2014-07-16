using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.truecolor.wp {
  [Table]
  internal sealed class DbModelActivity : INotifyPropertyChanged, INotifyPropertyChanging {
    // Define ID: private field, public property and database column.
    private int _id;
    [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity",
      CanBeNull = false, AutoSync = AutoSync.OnInsert)]
    public int Id {
      get { return _id; }
      set {
        if (_id != value) {
          NotifyPropertyChanging("Id");
          _id = value;
          NotifyPropertyChanged("Id");
        }
      }
    }

    private int _startAt;
    [Column]
    public int StartAt {
      get { return _startAt; }
      set {
        if (_startAt != value) {
          NotifyPropertyChanging("StartAt");
          _startAt = value;
          NotifyPropertyChanged("StartAt");
        }
      }
    }

    private int _endAt;
    [Column]
    public int EndAt {
      get { return _endAt; }
      set {
        if (_endAt != value) {
          NotifyPropertyChanging("EndAt");
          _endAt = value;
          NotifyPropertyChanged("EndAt");
        }
      }
    }


    #region INotifyPropertyChanged Members
    public event PropertyChangedEventHandler PropertyChanged;
    // Used to notify the page that a data context property changed
    private void NotifyPropertyChanged(string propertyName) {
      if (PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
    #endregion
    #region INotifyPropertyChanging Members
    public event PropertyChangingEventHandler PropertyChanging;
    // Used to notify the data context that a data context property is about to change
    private void NotifyPropertyChanging(string propertyName) {
      if (PropertyChanging != null) {
        PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
      }
    }
    #endregion
  }


  [Table]
  internal sealed class DbModelEvent : INotifyPropertyChanged, INotifyPropertyChanging {
    // Define ID: private field, public property and database column.
    private int _id;
    [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity",
      CanBeNull = false, AutoSync = AutoSync.OnInsert)]
    public int Id {
      get { return _id; }
      set {
        if (_id != value) {
          NotifyPropertyChanging("Id");
          _id = value;
          NotifyPropertyChanged("Id");
        }
      }
    }

    private string _name;
    [Column]
    public string Name {
      get { return _name; }
      set {
        if (_name != value) {
          NotifyPropertyChanging("Name");
          _name = value;
          NotifyPropertyChanged("Name");
        }
      }
    }

    private string _param;
    [Column]
    public string Param {
      get { return _param; }
      set {
        if (_param != value) {
          NotifyPropertyChanging("Param");
          _param = value;
          NotifyPropertyChanged("Param");
        }
      }
    }

    private string _value;
    [Column]
    public string Value {
      get { return _value; }
      set {
        if (_value != value) {
          NotifyPropertyChanging("Value");
          _value = value;
          NotifyPropertyChanged("Value");
        }
      }
    }

    private string _version;
    [Column]
    public string Version {
      get { return _version; }
      set {
        if (_version != value) {
          NotifyPropertyChanging("Version");
          _version = value;
          NotifyPropertyChanged("Version");
        }
      }
    }

    private int _createdAt;
    [Column]
    public int CreatedAt {
      get { return _createdAt; }
      set {
        if (_createdAt != value) {
          NotifyPropertyChanging("CreatedAt");
          _createdAt = value;
          NotifyPropertyChanged("CreatedAt");
        }
      }
    }



    #region INotifyPropertyChanged Members
    public event PropertyChangedEventHandler PropertyChanged;
    // Used to notify the page that a data context property changed
    private void NotifyPropertyChanged(string propertyName) {
      if (PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
    #endregion
    #region INotifyPropertyChanging Members
    public event PropertyChangingEventHandler PropertyChanging;
    // Used to notify the data context that a data context property is about to change
    private void NotifyPropertyChanging(string propertyName) {
      if (PropertyChanging != null) {
        PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
      }
    }
    #endregion
  }

  internal sealed class TCClickDataContext : DataContext {
    // Specify the connection string as a static, used in main page and app.xaml.
    private static string DBConnectionString = "Data Source=isostore:/tcclick.sdf";

    // Pass the connection string to the base class.
    public TCClickDataContext() : base(TCClickDataContext.DBConnectionString) { }

    public Table<DbModelActivity> DbModelActivities;
    public Table<DbModelEvent> DbModelEvents;
  }
}
