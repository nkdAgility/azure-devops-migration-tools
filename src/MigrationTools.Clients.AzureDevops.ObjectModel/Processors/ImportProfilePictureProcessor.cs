using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infra;
using MigrationTools.Tools;
using VstsSyncMigrator._EngineV1.Processors;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Downloads corporate images and updates TFS/Azure DevOps profiles
    /// </summary>
    /// <status>alpha</status>
    /// <processingtarget>Profiles</processingtarget>
    public class ImportProfilePictureContext : TfsStaticProcessorBase
    {
        private readonly IIdentityManagementService2 ims2;

        public ImportProfilePictureContext(TfsStaticTools tfsStaticEnrichers, StaticTools staticEnrichers, IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<StaticProcessorBase> logger) : base(tfsStaticEnrichers, staticEnrichers, services, me, telemetry, logger)
        {
            //http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C
            ims2 = (IIdentityManagementService2)me.Target.GetService<IIdentityManagementService2>();
        }

        public override string Name
        {
            get
            {
                return "ImportProfilePictureContext";
            }
        }

        public static string FriendlyDomainToLdapDomain(string friendlyDomainName)
        {
            string ldapPath = null;
            try
            {
                DirectoryContext objContext = new DirectoryContext(
                    DirectoryContextType.Domain, friendlyDomainName);
                Domain objDomain = Domain.GetDomain(objContext);
                ldapPath = objDomain.Name;
            }
            catch (DirectoryServicesCOMException e)
            {
                ldapPath = e.Message.ToString();
            }
            return ldapPath;
        }

        public bool ClearProfileImage(string identity, out string message)
        {
            bool ret = true;
            message = string.Empty;

            TeamFoundationIdentity i = ims2.ReadIdentity(IdentitySearchFactor.AccountName, identity, MembershipQuery.Direct, ReadIdentityOptions.None);

            if (i == null)
            {
                message = "User/Group [" + identity + "] not found";
                ret = false;
            }

            if (ret)
            {
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Data", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Type", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Id", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.Data", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.UploadDate", null);

                try
                {
                    ims2.UpdateExtendedProperties(i);
                }
                catch (PropertyServiceException)
                {
                    // swallow; this exception happens each and every time, but the changes are applied :S.
                }

                message = "Profile image cleared";
            }

            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to kill all errors")]
        public bool SetProfileImage(string identity, string imagePath, out string message)
        {
            bool ret = true;
            message = string.Empty;
            byte[] image = new byte[0];

            TeamFoundationIdentity i = ims2.ReadIdentity(IdentitySearchFactor.AccountName, identity, MembershipQuery.Direct, ReadIdentityOptions.None);

            if (i == null)
            {
                message = "User/Group [" + identity + "] not found";
                ret = false;
            }

            if (!File.Exists(imagePath))
            {
                message = "File [" + imagePath + "] not found";
                ret = false;
            }

            if (ret)
            {
                try
                {
                    byte[] rawImage = File.ReadAllBytes(imagePath);
                    image = ConvertAndResizeImage(rawImage);
                }
                catch (Exception ex)
                {
                    message = "Could not read the profile image: " + ex.Message;
                    ret = false;
                }
            }

            if (ret)
            {
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Data", image);
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Type", "image/png");
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Id", Guid.NewGuid().ToByteArray());
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.Data", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.UploadDate", null);

                try
                {
                    ims2.UpdateExtendedProperties(i);
                }
                catch (PropertyServiceException)
                {
                    // swallow; this exception happens each and every time, but the changes are applied :S.
                }

                message = "Profile image set";
            }

            return ret;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            string exportPath;
            string assPath = @"C:\Users\martinh\Downloads\mugshots\mugshots"; //System.Reflection.Assembly.GetEntryAssembly().Location;
                                                                              // exportPath = Path.Combine(Path.GetDirectoryName(assPath), "export-pic");
            exportPath = assPath;
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            var files = Directory.GetFiles(exportPath);
            var regex = new Regex(Regex.Escape("-"));
            foreach (string file in files)
            {
                string ident = regex.Replace(Path.GetFileNameWithoutExtension(file), @"\", 1);
                string mess;
                if (SetProfileImage(ident, file, out mess))
                {
                    Log.LogInformation(" [UPDATE] New Profile for : {0} ", ident);
                    File.Delete(file);
                }
                else
                {
                    Log.LogInformation(" [FAIL] Unable to set: {0} ", ident);
                }
            }

            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }

        private static byte[] ConvertAndResizeImage(byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length < 1))
            {
                throw new ArgumentException("The file could not be found.");
            }

            if (bytes.Length > 0x400000)
            {
                throw new ArgumentException("The file is too large to be used as profile image.");
            }

            using (var imageStream = new MemoryStream(bytes))
            using (Image image = Image.FromStream(imageStream))
            {
                int width = 0x90;
                int height = 0x90;
                if (image.Height > image.Width)
                {
                    width = (0x90 * image.Width) / image.Height;
                }
                else
                {
                    height = (0x90 * image.Height) / image.Width;
                }

                int x = (0x90 - width) / 2;
                int y = (0x90 - height) / 2;
                using (Bitmap bitmap = new Bitmap(0x90, 0x90))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.DrawImage(image, x, y, width, height);
                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        return stream.ToArray();
                    }
                }
            }
        }
    }
}