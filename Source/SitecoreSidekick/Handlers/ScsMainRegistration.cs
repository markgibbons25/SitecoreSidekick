using System;
using System.Collections.Specialized;
using SitecoreSidekick.Core;

namespace SitecoreSidekick.Handlers
{
	/// <summary>
	/// base handler for http requests to the SCS 
	/// </summary>
	public class ScsMainRegistration : ScsRegistration
	{
		public override string Directive => string.Empty;
		public override NameValueCollection DirectiveAttributes { get; set; }
		public override string Icon => "";
		public override string Name => "Sitecore Sidekick";
		public override string ResourcesPath => "SitecoreSidekick.Resources";
		public override string CssStyle => "600px";
		public ScsMainRegistration(string roles, string isAdmin, string users) : base(roles, isAdmin, users)
		{
		}
		public override string Identifier => "scs";
		public override Type Controller => typeof(ScsMainController);
	}
}
