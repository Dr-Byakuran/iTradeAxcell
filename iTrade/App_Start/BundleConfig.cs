using System.Web;
using System.Web.Optimization;

namespace iTrade
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/assets/plugins/jquery/jquery-1.9.1.min.js",
                        "~/assets/plugins/jquery/jquery-migrate-1.1.0.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
            "~/assets/plugins/jquery-ui/ui/minified/jquery-ui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/assets/plugins/bootstrap/js/bootstrap.min.js",
                      "~/assets/crossbrowserjs/respond.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/pace").Include(
                      "~/assets/plugins/pace/pace.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/slimscroll").Include(
                      "~/assets/plugins/slimscroll/jquery.slimscroll.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-cookie").Include(
                      "~/assets/plugins/jquery-cookie/jquery.cookie.js"));

            bundles.Add(new StyleBundle("~/css/css").Include(
                      "~/assets/plugins/jquery-ui/themes/base/minified/jquery-ui.min.css",
                      "~/assets/plugins/bootstrap/css/bootstrap.min.css",
                      "~/assets/plugins/font-awesome/css/font-awesome.min.css",
                      "~/assets/css/animate.min.css",
                      "~/assets/css/style.min.css",
                      "~/assets/css/style-responsive.min.css",
                      "~/assets/css/theme/blue.css",
                      "~/assets/plugins/simple-line-icons/simple-line-icons.css"
                      ));

            #region file upload
            bundles.Add(new StyleBundle("~/bootstrap-fileinput-master/filecss").Include("~/bootstrap-fileinput-master/css/fileinput.min.css"));
            bundles.Add(new ScriptBundle("~/bundles/filejs").Include("~/bootstrap-fileinput-master/js/fileinput.min.js",
"~/Scripts/MyFile.js"
                ));
            #endregion
            // BundleTable.EnableOptimizations = false;
        }

    }
}
