using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace IntegracionPAMI.WindowsService.SQL
{
	public partial class IntegracionPAMIWindowsServiceSQL : ServiceBase
	{
		public IntegracionPAMIWindowsServiceSQL()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
		}

		protected override void OnStop()
		{
		}
	}
}
