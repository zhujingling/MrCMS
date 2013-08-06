using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Indexing.Querying;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Website;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Web.Common;
using Ninject;

namespace MrCMS.IoC
{
    //Wires up IOC automatically
    public class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind(syntax => syntax.FromAssembliesMatching("MrCMS.*").SelectAllClasses()
                                      .Where(t => !typeof(SettingsBase).IsAssignableFrom(t) && !typeof(IController).IsAssignableFrom(t) && !Kernel.GetBindings(t).Any())
                                      .BindWith<NinjectServiceToInterfaceBinder>()
                                      .Configure(onSyntax => onSyntax.InRequestScope()));
            Kernel.Bind(syntax => syntax.FromAssembliesMatching("MrCMS.*").SelectAllClasses()
                                      .Where(t => typeof(SiteSettingsBase).IsAssignableFrom(t) && !typeof(IController).IsAssignableFrom(t) && !Kernel.GetBindings(t).Any())
                                      .BindWith<NinjectSiteSettingsBinder>()
                                      .Configure(onSyntax => onSyntax.InRequestScope()));

            Kernel.Bind<HttpRequestBase>().ToMethod(context => CurrentRequestData.CurrentContext.Request);
            Kernel.Bind<HttpSessionStateBase>().ToMethod(context => CurrentRequestData.CurrentContext.Session);
            Kernel.Bind<ObjectCache>().ToMethod(context => MemoryCache.Default);
            Kernel.Bind<Cache>().ToMethod(context => CurrentRequestData.CurrentContext.Cache);
            Kernel.Bind(typeof(ISearcher<,>)).To(typeof(FSDirectorySearcher<,>)).InRequestScope();
            Kernel.Rebind<Site>()
                  .ToMethod(context => CurrentRequestData.CurrentSite)
                  .InRequestScope();

            Kernel.Bind<HttpContextBase>()
                  .ToMethod(context => new HttpContextWrapper(HttpContext.Current))
                  .When(request => HttpContext.Current != null)
                  .InRequestScope();
            Kernel.Bind<HttpContextBase>()
                  .ToMethod(context => new OutOfContext())
                  .When(request => HttpContext.Current == null)
                  .InThreadScope();

            // Allowing IFileSystem implementation to be set in the site settings
            Kernel.Rebind<IFileSystem>().ToMethod(context =>
                                                      {
                                                          var storageType =
                                                              context.Kernel.Get<FileSystemSettings>().StorageType;
                                                          if (!string.IsNullOrWhiteSpace(storageType))
                                                          {
                                                              return context.Kernel.Get(
                                                                  TypeHelper.GetTypeByName(storageType)) as
                                                                     IFileSystem;
                                                          }
                                                          return context.Kernel.Get<FileSystem>();
                                                      }).InRequestScope();
        }
    }
}