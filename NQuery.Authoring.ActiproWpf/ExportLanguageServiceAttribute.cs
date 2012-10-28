using System;
using System.ComponentModel.Composition;

namespace NQuery.Authoring.ActiproWpf
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [MetadataAttribute]
    internal sealed class ExportLanguageServiceAttribute : ExportAttribute
    {
        private readonly Type _serviceType;

        public ExportLanguageServiceAttribute()
            : this(null)
        {
        }

        public ExportLanguageServiceAttribute(Type serviceType)
            : base(typeof(object))
        {
            _serviceType = serviceType;
        }

        public Type ServiceType
        {
            get { return _serviceType; }
        }
    }
}