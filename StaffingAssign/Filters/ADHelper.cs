using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;

namespace GetMembersFromGroup
{
    public class ADHelper
    {
        string UserPath = string.Empty;
        string CustomName = string.Empty;

        public bool CheckUser(string _user, string domainName)
        {
            try
            {

                DirectoryEntry de = new DirectoryEntry("LDAP://" + domainName);                
                DirectorySearcher dsearch = new DirectorySearcher(de);
                dsearch.Filter = String.Format("(&(objectClass=user)(objectCategory=person)(sAMAccountName={0}))", _user);
                dsearch.PropertiesToLoad.Add("DisplayName");
                
                SearchResultCollection results = dsearch.FindAll();

                if (results.Count > 0)
                {
                    UserPath = results[0].Path;
                    return true;
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public string GetUserCustomName(string _user, string domainName)
        {
            try
            {

                DirectoryEntry de = new DirectoryEntry("LDAP://" + domainName);
                DirectorySearcher dsearch = new DirectorySearcher(de);
                dsearch.Filter = String.Format("(&(objectClass=user)(objectCategory=person)(sAMAccountName={0}))", _user);
               
                dsearch.PropertiesToLoad.Add("displayname");

                SearchResultCollection results = dsearch.FindAll();

                if (results.Count > 0)
                {
                    CustomName = results[0].Properties["displayname"][0].ToString();
                    return CustomName;
                }
                else
                    return string.Empty;

            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }

        public bool AuthenticateUser(string domainName, string userName, string password)
        {
            bool ret = false;
            SearchResult results = null;
            try
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://" + domainName, userName, password);
                DirectorySearcher dsearch = new DirectorySearcher(de);

                dsearch.Filter = "(SAMAccountName=" + userName + ")";
                dsearch.PropertiesToLoad.Add("cn");

                results = dsearch.FindOne();

                ret = true;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public bool FindUserInGroup(string _user, string _group, string _domainName)
        {
            try
            {
                DirectoryEntry ent = new DirectoryEntry(GetGroupByName(_group, _domainName));
                DirectorySearcher dsearch = new DirectorySearcher(ent);
                dsearch.PropertiesToLoad.Add("member");

                SearchResult results = dsearch.FindOne();
                StringBuilder groupNames = new StringBuilder();

                if (results != null)
                {
                    int propertyCount = results.Properties["member"].Count;
                    string dn;
                    int equalsIndex, commaIndex;

                    for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                    {
                        dn = (string)results.Properties["member"][propertyCounter];
                        equalsIndex = dn.IndexOf("=", 1);
                        commaIndex = dn.IndexOf(",", 1);
                        if (-1 == equalsIndex)
                        {
                            return false;
                        }
                        groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                        groupNames.Append("|");
                    }
                }
                if (groupNames.ToString().ToUpper().Contains(GetUserCustomName(_user,_domainName).ToUpper()))
                    return true;

            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        public string GetGroupByName(string _groupName, string _domainName)
        {
            string strLDAP = string.Empty;

            try
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://" + _domainName);
                DirectorySearcher dsearch = new DirectorySearcher(de);
                //dsearch.Filter = "(CN=" + _groupName + ")";
                dsearch.Filter = "(&(objectcategory=group)(objectClass=group)(cn=" + _groupName + "*))";

                SearchResult results = dsearch.FindOne();

                strLDAP = results.Path;

            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return strLDAP;
        }

        public List<string> GetUsersFromGroup(string _groupName, string _domainName)
        {
            List<string> UsersList = new List<string>();

            DirectoryEntry de = new DirectoryEntry("LDAP://" + _domainName);
            DirectorySearcher dsearch = new DirectorySearcher(de);
            dsearch.Filter = "(CN=" + _groupName + ")";
            //dsearch.PropertiesToLoad.Add("displayname");
            //dsearch.PropertiesToLoad.Add("mail");
            SearchResult results = dsearch.FindOne();
            if (results != null)
            {
                for (int memberCount = 0; memberCount < results.Properties["member"].Count; memberCount++)
                {
                    var a = results.Properties["member"];
                    UsersList.Add(results.Properties["member"][memberCount].ToString());
                }
                return UsersList;
            }
            else
            {
                return null;
            }
            
        }

        public bool AddGroupUsers(string _user, string _group, string _domainName)
        {
            try
            {
                CheckUser(_user, _domainName);

                string[] memberPath = UserPath.Split('/');
                string member = memberPath[memberPath.Length - 1];

                DirectoryEntry ent = new DirectoryEntry(GetGroupByName(_group, "INTSURG"), "MXGroupAdministrator", "mxgr0upAdm!n");
                ent.Properties["member"].Add(member);
                ent.CommitChanges();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public bool RemoveGroupUsers(string _user, string _group, string _domainName)
        {
            try
            {
                CheckUser(_user, _domainName);

                string[] memberPath = UserPath.Split('/');
                string member = memberPath[memberPath.Length - 1];

                DirectoryEntry ent = new DirectoryEntry(GetGroupByName(_group, _domainName), "MXGroupAdministrator", "mxgr0upAdm!n");
                ent.Properties["member"].Remove(member);
                ent.CommitChanges();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public List<string> GetUserInfo(string _user, string _domainName)
        {

            List<string> userInfo = new List<string>();
            DirectoryEntry de = new DirectoryEntry("LDAP://" + _domainName);
            DirectorySearcher dsearch = new DirectorySearcher(de);
            dsearch.Filter = String.Format("(&(objectClass=user)(objectCategory=person)(sAMAccountName={0}))", _user);
            dsearch.PropertiesToLoad.Add("displayname");
            dsearch.PropertiesToLoad.Add("mail");
            dsearch.PropertiesToLoad.Add("title");
            dsearch.PropertiesToLoad.Add("department");
            dsearch.PropertiesToLoad.Add("collinscostcenter");

            try
            {
                SearchResultCollection results = dsearch.FindAll();

                if (results.Count > 0)
                {
                    userInfo.Add(results[0].Properties["displayName"][0].ToString());
                    userInfo.Add(results[0].Properties["mail"][0].ToString());
                    userInfo.Add(results[0].Properties["title"][0].ToString());
                    userInfo.Add(results[0].Properties["department"][0].ToString());
                    userInfo.Add(results[0].Properties["collinscostcenter"][0].ToString());
                    return userInfo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return userInfo;
            }
        }

        public bool AddUser(string _newUser, string _firstName, string _lastName, string _domainName)
        {
            try
            {
                DirectoryEntry parent = new DirectoryEntry("LDAP://OU=Mexicali,OU=People,OU=Collins,DC=rcmnet,DC=rockwellcollins,DC=com",
                    null, null, AuthenticationTypes.Secure);
                using (parent)
                {
                    DirectoryEntry user = parent.Children.Add("CN=" + _newUser, "user");

                    user.Properties["sAMAccountName"].Value = _newUser.ToLower();
                    user.Properties["userPrincipalName"].Value = _newUser + "@Rcmnet.Rockwellcollins.com";
                    user.Properties["givenName"].Value = _firstName;
                    user.Properties["name"].Value = _newUser;
                    user.Properties["sn"].Value = _lastName;
                    user.Properties["displayname"].Value = _firstName + " " + " " + _lastName;
                    user.Properties["description"].Value = "RESP-" + _newUser;
                    user.Properties["mail"].Value = _newUser + "@Rockwellcollins.com";
                    user.CommitChanges();

                    string DefPassword = "Rock*001";
                    user.Invoke("SetPassword", new object[] { DefPassword });
                    user.Properties["userAccountControl"].Value = 512;//set user account enabled
                    user.CommitChanges();

                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public string GetUserIdFromDisplayName(string displayName)
        {
            // set up domain context
            try
            {
                using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
                {
                    // find user by display name
                    UserPrincipal user = UserPrincipal.FindByIdentity(ctx, displayName);

                    // 
                    if (user != null)
                    {
                        return user.SamAccountName;
                        // or maybe you need user.UserPrincipalName;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch(Exception ex)
            {
                return null;
            }
            
        }
    }
}
