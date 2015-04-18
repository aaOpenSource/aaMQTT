using System;
using Newtonsoft.Json;

namespace aaMXItem
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MXItem
    {
        private int _hLMXServerHandle;
        private int _phItemHandle;
        private object _pvItemValue;
        private int _pwItemQuality;
        private object _pftItemTimeStamp;
        private ArchestrA.MxAccess.MXSTATUS_PROXY[] _ItemStatus;
        private string _TagName;

        public MXItem()
        {
            _hLMXServerHandle = 0;
            _phItemHandle = 0;
            _pvItemValue = 0;
            _pwItemQuality = 0;
            _pftItemTimeStamp = "1/1/1970";
            _TagName = "";
        }

        public MXItem(int ItemHandle)
        {
            _hLMXServerHandle = 0;
            _phItemHandle = ItemHandle;
            _pvItemValue = 0;
            _pwItemQuality = 0;
            _pftItemTimeStamp = "1/1/1970";
            _TagName = "";
        }

        [JsonProperty]
        public int ServerHandle
        {
            get
            {
                return _hLMXServerHandle;
            }
            set
            {
                _hLMXServerHandle = value;
            }

        }

        [JsonProperty]
        public int ItemHandle
        {
            get
            {
                return _phItemHandle;
            }
            set
            {
                _phItemHandle = value;
            }
        }

        [JsonProperty]
        public object Value
        {
            get
            {
                return _pvItemValue;
            }
            set
            {
                _pvItemValue = value;
            }
        }

        [JsonProperty]
        public int Quality
        {
            get
            {
                return _pwItemQuality;
            }
            set
            {
                _pwItemQuality = value;
            }
        }

        [JsonProperty]
        public DateTime TimeStamp
        {
            get
            {
                return Convert.ToDateTime(_pftItemTimeStamp);
            }
            set
            {
                _pftItemTimeStamp = (object)value;
            }
        }

        [JsonProperty]
        public ArchestrA.MxAccess.MXSTATUS_PROXY[] Status
        {
            get
            {
                return _ItemStatus;
            }
            set
            {
                _ItemStatus = value;
            }
        }

        [JsonProperty]
        public string TagName
        {
            get
            {
                return _TagName;
            }
            set
            {
                _TagName = value;
            }
        }

        public string ToJSON()
        {
            return this.ToJSON(false);
        }

        public string ToJSON(bool indented)
        {
            if (indented)
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            else
            {
                return JsonConvert.SerializeObject(this, Formatting.None);
            }
        }
    }
}
