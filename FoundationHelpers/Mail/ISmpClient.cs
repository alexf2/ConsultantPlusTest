using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifx.FoundationHelpers.Mail
{
	public interface ISmpClient: IDisposable
	{
		void Send (IMailMessage msg);
	}
}
