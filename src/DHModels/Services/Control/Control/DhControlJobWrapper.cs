namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Wrapper.Attributes;
using Wrapper.Base;
using Wrapper.Shared;
using Wrapper.Util;
using Wrapper.Validators;

[EntityWrapper(EntityCategory.Control)]
public sealed class DhControlJobWrapper : ContainerEntityDynamicWrapper<DhControlJobWrapper>
{
    private const string KeyJobId = "jobId";
    private const string KeyCreatedTime = "createdTime";
    private const string KeyJobStatus = "jobStatus";
    private const string KeyStorageEndpoint = "storageEndpoint";
    private const string KeyInputs = "inputs";
    private const string KeyEvaluations = "evaluations";
    private const string KeyType = "type";

    public DhControlJobWrapper() : this(new JObject()) { }

    public DhControlJobWrapper(JObject jObject) : base(jObject)
    {
        this.Type = DhControlJobWrapperType.Job;
    }

    [JsonProperty("id")]
    public override string Id
    {
        get => base.Id;
        set => base.Id = value;
    }

    [JsonProperty("tenantId")]
    public override string? TenantId
    {
        get
        {
            string? tenantId = base.TenantId;
            if (tenantId != null)
            {
                return tenantId;
            }

            throw new ArgumentNullException("tenantId");
        }
        set => base.TenantId = value;
    }
    
    [JsonProperty("TenantId")]
    public string TenantIdUpper => this.TenantId ?? throw new InvalidOperationException();

    [JsonProperty("accountId")]
    public override string? AccountId
    {
        get => base.AccountId ?? throw new ArgumentNullException("AccountId");
        set => base.AccountId = value;
    }

    [JsonProperty("systemData")]
    public override SystemDataWrapper SystemData
    {
        get => base.SystemData;
        set => base.SystemData = SystemDataWrapperPascalCase.FromBase(value);
    }

    [EntityProperty(KeyType)]
    [JsonProperty("type")]
    public override string Type
    {
        get => base.Type;
        set => base.Type = value;
    }

    [EntityProperty(KeyJobId)]
    [EntityRequiredValidator]
    [JsonProperty("jobId")]
    public string JobId
    {
        get => this.GetPropertyValue<string>(KeyJobId);
        set => this.SetPropertyValue(KeyJobId, value);
    }

    [EntityProperty(KeyCreatedTime)]
    [JsonProperty("createdTime")]
    public DateTime CreatedTime
    {
        get => this.GetPropertyValue<DateTime>(KeyCreatedTime);
        set => this.SetPropertyValue(KeyCreatedTime, value);
    }

    [EntityProperty(KeyJobStatus)]
    [JsonProperty("jobStatus")]
    public string JobStatus
    {
        get => this.GetPropertyValue<string>(KeyJobStatus);
        set => this.SetPropertyValue(KeyJobStatus, value);
    }

    [EntityProperty(KeyStorageEndpoint)]
    [EntityRequiredValidator]
    [JsonProperty("storageEndpoint")]
    public string StorageEndpoint
    {
        get => this.GetPropertyValue<string>(KeyStorageEndpoint);
        set => this.SetPropertyValue(KeyStorageEndpoint, value);
    }

    private List<InputsWrapper>? inputs;

    [EntityProperty(KeyInputs)]
    [JsonProperty("inputs")]
    public List<InputsWrapper> Inputs
    {
        get
        {
            if (this.inputs != null)
            {
                return this.inputs;
            }

            var pathsArray = this.GetPropertyValue<JArray>(KeyInputs);
            this.inputs = pathsArray?.Select(x => new InputsWrapper((JObject)x))
                .ToList() ?? [];

            return this.inputs;
        }
        set
        {
            var pathsArray = new JArray(value.Select(x => x.JObject));
            this.SetPropertyValue(KeyInputs, pathsArray);
            this.inputs = value;
        }
    }

    public sealed class OutputsWrapper : DynamicEntityWrapper
    {
        private const string KeyType = "type";
        private const string KeyTypeProperties = "typeProperties";

        public OutputsWrapper() : this(new JObject()) { }

        public OutputsWrapper(JObject jObject) : base(jObject)
        {
            this.Type = "AdlsGen2FileLocation";
        }

        [EntityProperty(KeyType)]
        [EntityRequiredValidator]
        [JsonProperty("type")]
        public override string Type
        {
            get => this.GetPropertyValue<string>(KeyType);
            set => this.SetPropertyValue(KeyType, value);
        }

        private OutputPathTypePropertiesWrapper? typeProperties;

        [EntityProperty(KeyTypeProperties)]
        [JsonProperty("typeProperties")]
        public new OutputPathTypePropertiesWrapper TypeProperties
        {
            get
            {
                if (this.typeProperties != null)
                {
                    return this.typeProperties;
                }

                var props = this.GetPropertyValue<JObject>(KeyTypeProperties);
                this.typeProperties = props != null
                    ? new OutputPathTypePropertiesWrapper(props)
                    : new OutputPathTypePropertiesWrapper(new JObject());

                return this.typeProperties;
            }
            set
            {
                this.SetPropertyValue(KeyTypeProperties, value.JObject);
                this.typeProperties = value;
            }
        }
    }

    public class SystemDataWrapperPascalCase : SystemDataWrapper
    {
        [JsonProperty("createdBy")]
        public override string CreatedBy
        {
            get => this.GetPropertyValue<string>(KeyCreatedBy);
            set => this.SetPropertyValue(KeyCreatedBy, value);
        }

        [JsonProperty("createdAt")]
        public override DateTime? CreatedAt
        {
            get => this.GetPropertyValue<DateTime?>(KeyCreatedAt);
            set => this.SetPropertyValue(KeyCreatedAt, value?.GetDateTimeStr());
        }

        [JsonProperty("lastModifiedBy")]
        public override string LastModifiedBy
        {
            get => this.GetPropertyValue<string>(KeyLastModifiedBy);
            set => this.SetPropertyValue(KeyLastModifiedBy, value);
        }

        [JsonProperty("lastModifiedAt")]
        public override DateTime? LastModifiedAt
        {
            get => this.GetPropertyValue<DateTime?>(KeyLastModifiedAt);
            set => this.SetPropertyValue(KeyLastModifiedAt, value);
        }

        public static SystemDataWrapperPascalCase FromBase(SystemDataWrapper baseObject)
        {
            return new SystemDataWrapperPascalCase { CreatedBy = baseObject.CreatedBy, CreatedAt = baseObject.CreatedAt, LastModifiedBy = baseObject.LastModifiedBy, LastModifiedAt = baseObject.LastModifiedAt };
        }
    }

    public class OutputPathTypePropertiesWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
    {
        private const string KeyAccount = "account";
        private const string KeyFileSystem = "fileSystem";
        private const string KeyPath = "path";

        public OutputPathTypePropertiesWrapper() : this(new JObject()) { }

        [EntityProperty(KeyAccount)]
        [EntityRequiredValidator]
        [JsonProperty("account")]
        public string Account
        {
            get => this.GetPropertyValue<string>(KeyAccount);
            set => this.SetPropertyValue(KeyAccount, value);
        }

        [EntityProperty(KeyFileSystem)]
        [EntityRequiredValidator]
        [JsonProperty("fileSystem")]
        public string FileSystem
        {
            get => this.GetPropertyValue<string>(KeyFileSystem);
            set => this.SetPropertyValue(KeyFileSystem, value);
        }

        [EntityProperty(KeyPath)]
        [EntityRequiredValidator]
        [JsonProperty("path")]
        public string Path
        {
            get => this.GetPropertyValue<string>(KeyPath);
            set => this.SetPropertyValue(KeyPath, value);
        }
    }

    private List<DhControlJobEvaluationsWrapper>? evaluations;

    [EntityProperty(KeyEvaluations)]
    [JsonProperty("evaluations")]
    public List<DhControlJobEvaluationsWrapper> Evaluations
    {
        get
        {
            if (this.evaluations != null)
            {
                return this.evaluations;
            }

            var evaluationsArray = this.GetPropertyValue<JArray>(KeyEvaluations);
            this.evaluations = evaluationsArray?.Select(x => new DhControlJobEvaluationsWrapper((JObject)x))
                .ToList() ?? [];

            return this.evaluations;
        }
        set
        {
            var evaluationsArray = new JArray(value.Select(x => x.JObject));
            this.SetPropertyValue(KeyEvaluations, evaluationsArray);
            this.evaluations = value;
        }
    }

    public override void OnUpdate(DhControlJobWrapper existWrapper, string userId)
    {
        base.OnUpdate(existWrapper, userId);
        this.JobId = existWrapper.JobId;
        this.CreatedTime = existWrapper.CreatedTime;
    }
}

public class InputsWrapper : DynamicEntityWrapper
{
    private const string KeyType = "type";
    private const string KeyTypeProperties = "typeProperties";

    public InputsWrapper() : this(new JObject()) { }

    public InputsWrapper(JObject jObject) : base(jObject)
    {
        this.Type = "AdlsGen2FileLocation";
    }

    [EntityProperty(KeyType)]
    [EntityRequiredValidator]
    [JsonProperty("type")]
    public override string Type
    {
        get => this.GetPropertyValue<string>(KeyType);
        set => this.SetPropertyValue(KeyType, value);
    }

    private InputPropertiesWrapper? typeProperties;

    [EntityProperty(KeyTypeProperties)]
    [JsonProperty("typeProperties")]
    public new InputPropertiesWrapper TypeProperties
    {
        get
        {
            if (this.typeProperties != null)
            {
                return this.typeProperties;
            }

            var props = this.GetPropertyValue<JObject>(KeyTypeProperties);
            this.typeProperties = props != null
                ? new InputPropertiesWrapper(props)
                : new InputPropertiesWrapper(new JObject());

            return this.typeProperties;
        }
        set
        {
            this.SetPropertyValue(KeyTypeProperties, value.JObject);
            this.typeProperties = value;
        }
    }

    public static InputsWrapper Create()
    {
        return new InputsWrapper { TypeProperties = new InputPropertiesWrapper() };
    }
}

public class InputPropertiesWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    private const string KeyFileSystem = "fileSystem";
    private const string KeyFolderPath = "folderPath";
    private const string KeyDatasourceFqn = "datasourceFQN";

    public InputPropertiesWrapper() : this(new JObject()) { }

    [EntityProperty(KeyFileSystem)]
    [EntityRequiredValidator]
    [JsonProperty("fileSystem")]
    public string FileSystem
    {
        get => this.GetPropertyValue<string>(KeyFileSystem);
        set => this.SetPropertyValue(KeyFileSystem, value);
    }

    [EntityProperty(KeyFolderPath)]
    [EntityRequiredValidator]
    [JsonProperty("folderPath")]
    public string FolderPath
    {
        get => this.GetPropertyValue<string>(KeyFolderPath);
        set => this.SetPropertyValue(KeyFolderPath, value);
    }

    [EntityProperty(KeyDatasourceFqn)]
    [EntityRequiredValidator]
    [JsonProperty("datasourceFQN")]
    public string DatasourceFqn
    {
        get => this.GetPropertyValue<string>(KeyDatasourceFqn);
        set => this.SetPropertyValue(KeyDatasourceFqn, value);
    }
}

public class DhControlJobEvaluationsWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    private const string KeyControlId = "controlId";
    private const string KeyQuery = "query";
    private const string KeyRules = "rules";

    public DhControlJobEvaluationsWrapper() : this(new JObject()) { }

    [EntityProperty(KeyControlId)]
    [EntityRequiredValidator]
    [JsonProperty("controlId")]
    public string ControlId
    {
        get => this.GetPropertyValue<string>(KeyControlId);
        set => this.SetPropertyValue(KeyControlId, value);
    }

    [EntityProperty(KeyQuery)]
    [EntityRequiredValidator]
    [JsonProperty("query")]
    public string Query
    {
        get => this.GetPropertyValue<string>(KeyQuery);
        set => this.SetPropertyValue(KeyQuery, value);
    }

    private List<DhControlJobRuleWrapper>? rules;

    [EntityProperty(KeyRules)]
    [JsonProperty("rules")]
    public List<DhControlJobRuleWrapper> Rules
    {
        get
        {
            if (this.rules != null)
            {
                return this.rules;
            }

            var rulesArray = this.GetPropertyValue<JArray>(KeyRules);
            this.rules = rulesArray?.Select(x => new DhControlJobRuleWrapper((JObject)x))
                .ToList() ?? [];

            return this.rules;
        }
        set
        {
            var rulesArray = new JArray(value.Select(x => x.JObject));
            this.SetPropertyValue(KeyRules, rulesArray);
            this.rules = value;
        }
    }
}

public class DhControlJobRuleWrapper(JObject jObject) : DynamicEntityWrapper(jObject)
{
    private const string KeyId = "id";
    private const string KeyName = "name";
    private const string KeyType = "type";
    private const string KeyCondition = "condition";

    public DhControlJobRuleWrapper() : this(new JObject()) { }

    [EntityProperty(KeyId)]
    [EntityRequiredValidator]
    [JsonProperty("id")]
    public string Id
    {
        get => this.GetPropertyValue<string>(KeyId);
        set => this.SetPropertyValue(KeyId, value);
    }

    [EntityProperty(KeyName)]
    [EntityRequiredValidator]
    [JsonProperty("name")]
    public string Name
    {
        get => this.GetPropertyValue<string>(KeyName);
        set => this.SetPropertyValue(KeyName, value);
    }

    [EntityProperty(KeyType)]
    [EntityRequiredValidator]
    [JsonProperty("type")]
    public override string Type
    {
        get => this.GetPropertyValue<string>(KeyType);
        set => this.SetPropertyValue(KeyType, value);
    }

    [EntityProperty(KeyCondition)]
    [EntityRequiredValidator]
    [JsonProperty("condition")]
    public string Condition
    {
        get => this.GetPropertyValue<string>(KeyCondition);
        set => this.SetPropertyValue(KeyCondition, value);
    }
}

public static class DhControlJobWrapperType
{
    public const string Job = "ControlJob";
}