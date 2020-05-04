using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlocklyGame.Managers
{
	public class LocalizationManager : RequestCultureProvider
	{
		public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
			//Go away and do a bunch of work to find out what culture we should do. 	

			//Return a provider culture result. 

			return await Task.FromResult(new ProviderCultureResult("sk-SK"));		

			//In the event I can't work out what culture I should use. Return null. 
			//Code will fall to other providers in the list OR use the default. 
			//return null;
		}


	}
}

namespace Localization
{
	public class SharedResource
	{

	}
}