﻿/**
 * Copyright (c) Marcus Kirsch (aka Marck). All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 *     * Redistributions of source code must retain the above copyright notice, 
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright notice, 
 *       this list of conditions and the following disclaimer in the documentation 
 *       and/or other materials provided with the distribution.
 *     * Neither the name of the Organizations nor the names of Individual
 *       Contributors may be used to endorse or promote products derived from 
 *       this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
 * THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED 
 * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */

using System.Collections.Generic;
using System.Xml;
using OpenMetaverse;

using GridRegion = OpenSim.Services.Interfaces.GridRegion;
using System.Collections.Specialized;

namespace Diva.Wifi
{
    public partial class Services
    {
        public string ConsoleRequest(Environment env)
        {
            m_log.DebugFormat("[Wifi]: ConsoleRequest");
            SessionInfo sinfo;
            if (TryGetSessionInfo(env.Request, out sinfo) &&
                (sinfo.Account.UserLevel >= WebApp.AdminUserLevel))
            {
                env.Session = sinfo;
                env.Flags = Flags.IsLoggedIn | Flags.IsAdmin;
                env.State = State.Console;
                return PadURLs(env, sinfo.Sid, m_WebApp.ReadFile(env, "index.html"));
            }

            return m_WebApp.ReadFile(env, "index.html");
        }

        public string ConsoleDataRequest(Environment env)
        {
            m_log.Debug("[Wifi]: ConsoleDataRequest");
            string result = string.Empty;

            SessionInfo sinfo;
            if (TryGetSessionInfo(env.Request, out sinfo) &&
                (sinfo.Account.UserLevel >= WebApp.AdminUserLevel))
            {
                // Create an XML document with the result data (console user and password)
                XmlDocument xmldoc = new XmlDocument();
                XmlNode xmlnode = xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                xmldoc.AppendChild(xmlnode);

                XmlElement rootElement = xmldoc.CreateElement("Wifi");
                xmldoc.AppendChild(rootElement);

                XmlElement consoleElement = xmldoc.CreateElement("Console");
                consoleElement.SetAttribute("User", m_WebApp.ConsoleUser);
                consoleElement.SetAttribute("Password", m_WebApp.ConsolePass);
                rootElement.AppendChild(consoleElement);

                result = xmldoc.InnerXml;
            }
            return result;
        }
    }
}
