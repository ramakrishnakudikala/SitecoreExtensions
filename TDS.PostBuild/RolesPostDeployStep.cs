﻿using System;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Sitecore.Security.Accounts;
using HedgehogDevelopment.SitecoreProject.PackageInstallPostProcessor.Contracts;

namespace TDS.PostDeploy
{
    public class RolesPostDeployStep : IPostDeployAction
    {
        public void RunPostDeployAction(XDocument deployedItems, IPostDeployActionHost host, string parameter)
        {
            var configuredRoleMappings = parameter;
            try
            {
                if (string.IsNullOrWhiteSpace(configuredRoleMappings))
                    return;

                var roleMappingsJson = JObject.Parse(configuredRoleMappings);

                foreach (XElement xelement in
                         System.Xml.XPath.Extensions.XPathSelectElements((XNode)deployedItems, "/DeployedItems/Role"))
                {
                    var roleNameAttribute = xelement.Attribute((XName)"Name");
                    if (roleNameAttribute != null)
                    {
                        var installedRoleName = roleNameAttribute.Value;
                        if (Sitecore.Security.Accounts.Role.Exists(installedRoleName))
                        {
                            var installedRole = Role.FromName(installedRoleName);
                            var targetRoleName = (string)roleMappingsJson[installedRoleName];
                            if (string.IsNullOrEmpty(targetRoleName))
                                continue;
                            var targetRoleMember = Role.FromName(targetRoleName);
                            RolesInRolesManager.AddRoleToRole(installedRole, targetRoleMember);
                            host.LogMessage("Added the role {0} as a member of the role {1}", installedRole.Name, targetRoleMember.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                host.LogMessage("Error in the post deploy action : {0}", ex.StackTrace);
            }

        }
    }
}
