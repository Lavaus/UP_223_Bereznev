using System;
using System.Windows;
using UP1.Windows;

namespace UP1
{
    public partial class App : Application
    {
        public static Read_and_writeEntities1 Db { get; private set; } = new Read_and_writeEntities1();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Db.Database.Connection.Open();
            Db.Database.Connection.Close();
            
            
        }
    }
}