using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.Services.Common.Internal;
using Microsoft.VisualStudio.Services.ServiceEndpoints.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json.Linq;

namespace MigrationTools.DataContracts.Pipelines
{
    [ApiPath("serviceendpoint/endpoints")]
    [ApiName("Service Connections")]
    public partial class ServiceConnection : RestApiDefinition
    {
        [DataMember(EmitDefaultValue = false, Name = "Data")]
        private Dictionary<string, string> m_data;

        // Summary: Gets or sets the type of the endpoint.
        [DataMember(EmitDefaultValue = false)]
        public string Type
        {
            get;
            set;
        }

        // Summary: Gets or sets the url of the endpoint.
        [DataMember(EmitDefaultValue = false)]
        public Uri Url
        {
            get;
            set;
        }

        // Summary: Gets or sets the identity reference for the user who created the Service endpoint.
        [DataMember(EmitDefaultValue = false)]
        public IdentityRef CreatedBy
        {
            get;
            set;
        }

        // Summary: Gets or sets the description of endpoint.
        [DataMember(EmitDefaultValue = false)]
        public string Description
        {
            get;
            set;
        }

        // Summary: Gets or sets the authorization data for talking to the endpoint.
        [DataMember(EmitDefaultValue = false)]
        public EndpointAuthorization Authorization
        {
            get;
            set;
        }

        // Summary: This is a deprecated field.
        [DataMember(EmitDefaultValue = false)]
        public Guid GroupScopeId
        {
            get;
            internal set;
        }

        // Summary: Gets or sets the identity reference for the administrators group of the service endpoint.
        [DataMember(EmitDefaultValue = false)]
        public IdentityRef AdministratorsGroup
        {
            get;
            internal set;
        }

        // Summary: Gets or sets the identity reference for the readers group of the service endpoint.
        [DataMember(EmitDefaultValue = false)]
        public IdentityRef ReadersGroup
        {
            get;
            internal set;
        }

        // Summary: Gets the custom data associated with this endpoint.
        public IDictionary<string, string> Data
        {
            get
            {
                return m_data;
            }
            set
            {
                if (value != null)
                {
                    m_data = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        // Summary: Indicates whether service endpoint is shared with other projects or not.
        [DataMember(EmitDefaultValue = true)]
        public bool IsShared
        {
            get;
            set;
        }

        // Summary: EndPoint state indicator
        [DataMember(EmitDefaultValue = true)]
        public bool IsReady
        {
            get;
            set;
        }

        // Summary: Error message during creation/deletion of endpoint
        [DataMember(EmitDefaultValue = false)]
        public JObject OperationStatus
        {
            get;
            set;
        }

        // Summary: Owner of the endpoint Supported values are "library", "agentcloud"
        [DataMember(EmitDefaultValue = false)]
        public string Owner
        {
            get;
            set;
        }

        public static bool ValidateServiceEndpoint(ServiceEndpoint endpoint, ref string message)
        {
            if (endpoint == null)
            {
                message = "endpoint: null";
                return false;
            }

            if (endpoint.Id == Guid.Empty)
            {
                message = CommonResources.EmptyGuidNotAllowed("endpoint.Id");
                return false;
            }

            if (string.IsNullOrEmpty(endpoint.Name))
            {
                message = string.Format("{0}:{1}", CommonResources.EmptyStringNotAllowed(), "endpoint.Name");
                return false;
            }

            if (endpoint.Url == null)
            {
                message = "endpoint.Url: null";
                return false;
            }

            if (string.IsNullOrEmpty(endpoint.Type))
            {
                message = string.Format("{0}:{1}", CommonResources.EmptyStringNotAllowed(), "endpoint.Type");
                return false;
            }

            if (endpoint.Authorization == null)
            {
                message = "endpoint.Authorization: null";
                return false;
            }

            return true;
        }

        public override void ResetObject()
        {
            SetSourceId(Id);

            //Replace null Keys since it can't be null
            var authParameters = Authorization.Parameters;
            var dataParameters = Data;
            bool hasNullKey = false;
            List<KeyValuePair<string, string>> NullKeyPair = new List<KeyValuePair<string, string>>();
            foreach (var param in authParameters)
            {
                if (param.Value == null)
                {
                    NullKeyPair.Add(param);
                    hasNullKey = true;
                }
            }
            if (hasNullKey)
            {
                foreach (var pair in NullKeyPair)
                {
                    authParameters.Remove(pair.Key);
                    authParameters.Add(pair.Key, "toBeReplaced");
                }
            }
            if (dataParameters.TryGetValue("environment", out var environment) && environment == "AzureCloud" &&
                dataParameters.TryGetValue("creationMode", out var creationMode) && creationMode == "Automatic")
            {
                authParameters.Remove("serviceprincipalid");
                authParameters.Remove("serviceprincipalkey");
                dataParameters.Remove("azureSpnRoleAssignmentId");
                dataParameters.Remove("azureSpnPermissions");
                dataParameters.Remove("spnObjectId");
                dataParameters.Remove("appObjectId");
            }

        }

        public override bool HasTaskGroups()
        {
            return false;
        }

        public override bool HasVariableGroups()
        {
            return false;
        }
    }
}