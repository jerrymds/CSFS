using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;

namespace CTCB.NUMS.ConfigEncrypt
{
    public class EncryptConfig
    {
        public void ToggleWebConnectStringEncrypt(string webConfigPath)
        {
            // Open the Web.config file.
            Configuration config = WebConfigurationManager.OpenWebConfiguration(webConfigPath);

            // Get the connectionStrings section.
            ConnectionStringsSection section = config.GetSection("connectionStrings")
                as ConnectionStringsSection;

            // Toggle encryption.
            if (section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();
            }
            else
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
            }

            // Save changes to the Web.config file.
            config.Save();

            //logger.WriteLogs2("Web.Config ConnectionStrings Protected=" + section.SectionInformation.IsProtected, _enyLogPath);
        }

        public void ToggleWebAppSettingEncrypt(string webConfigPath)
        {
            // Open the Web.config file.
            Configuration config = WebConfigurationManager.OpenWebConfiguration(webConfigPath);

            // Get the AppSettingsSection section.
            AppSettingsSection section = config.GetSection("appSettings") as AppSettingsSection;

            // Toggle encryption.
            if (section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();
            }
            else
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
            }

            // Save changes to the Web.config file.
            config.Save();

            //logger.WriteLogs("Web.Config appSetting Protected=" + section.SectionInformation.IsProtected);
        }

        public void ToggleConfigEncryption(string exeConfigName)
        {
            // Takes the executable file name without the
            // .config extension.
            try
            {
                // Open the configuration file and retrieve 
                // the connectionStrings section.
                Configuration config = ConfigurationManager.
                    OpenExeConfiguration(exeConfigName);

                //ConnectionStringsSection section =
                //    config.GetSection("connectionStrings")
                //    as ConnectionStringsSection;
                AppSettingsSection section = config.GetSection("appSettings") as AppSettingsSection;

                if (section.SectionInformation.IsProtected)
                {
                    // Remove encryption.
                    section.SectionInformation.UnprotectSection();
                }
                else
                {
                    // Encrypt the section.
                    section.SectionInformation.ProtectSection(
                        "DataProtectionConfigurationProvider");
                }
                // Save the current configuration.
                config.Save();

                //logger.WriteLogs("Protected=" + section.SectionInformation.IsProtected);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConnectionStringsEncryption(string exeConfigName)
        {
            // Takes the executable file name without the
            // .config extension.
            try
            {
                // Open the configuration file and retrieve 
                // the connectionStrings section.
                //Configuration config = ConfigurationManager.
                //    OpenExeConfiguration(exeConfigName);

                Configuration config = null;
                if (exeConfigName.Contains(".dll"))
                {
                    //加密*.dll.Config檔案
                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = exeConfigName + ".config" };
                    config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);//exeConfigName);                
                }
                else
                {
                    //加密*.exe.Config檔案
                    config = ConfigurationManager.
                        OpenExeConfiguration(exeConfigName);
                }

                ConnectionStringsSection section =
                    config.GetSection("connectionStrings")
                    as ConnectionStringsSection;

                if (section.SectionInformation.IsProtected)
                {
                    // Remove encryption.
                    section.SectionInformation.UnprotectSection();
                }
                else
                {
                    // Encrypt the section.
                    section.SectionInformation.ProtectSection(
                        "DataProtectionConfigurationProvider");
                }
                // Save the current configuration.
                config.Save();

                //logger.WriteLogs("App.Config ConnectionStrings Protected=" + section.SectionInformation.IsProtected);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AppSettingsEncryption(string exeConfigName)
        {
            // Takes the executable file name without the
            // .config extension.
            try
            {
                // Open the configuration file and retrieve 
                // the connectionStrings section.
                Configuration config = null;
                if (exeConfigName.Contains(".dll"))
                {
                    //加密*.dll.Config檔案
                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = exeConfigName + ".config" };
                    config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);//exeConfigName);                
                }
                else
                {
                    //加密*.exe.Config檔案
                    config = ConfigurationManager.
                        OpenExeConfiguration(exeConfigName);
                }

                AppSettingsSection section = config.GetSection("appSettings") as AppSettingsSection;

                if (section.SectionInformation.IsProtected)
                {
                    // Remove encryption.
                    section.SectionInformation.UnprotectSection();
                }
                else
                {
                    // Encrypt the section.
                    section.SectionInformation.ProtectSection(
                        "DataProtectionConfigurationProvider");
                }
                // Save the current configuration.
                config.Save();

                //logger.WriteLogs("App.Config appSettings Protected=" + section.SectionInformation.IsProtected);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public void AppSettingsEncryption(string exeConfigName)
        //{
        //    // Takes the executable file name without the
        //    // .config extension.
        //    try
        //    {
        //        // Open the configuration file and retrieve 
        //        // the connectionStrings section.
        //        Configuration config = ConfigurationManager.
        //            OpenExeConfiguration(exeConfigName);

        //        AppSettingsSection section = config.GetSection("appSettings") as AppSettingsSection;

        //        if (section.SectionInformation.IsProtected)
        //        {
        //            // Remove encryption.
        //            section.SectionInformation.UnprotectSection();
        //        }
        //        else
        //        {
        //            // Encrypt the section.
        //            section.SectionInformation.ProtectSection(
        //                "DataProtectionConfigurationProvider");
        //        }
        //        // Save the current configuration.
        //        config.Save();

        //        //logger.WriteLogs("App.Config appSettings Protected=" + section.SectionInformation.IsProtected);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}
