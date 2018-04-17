using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestViewer
{
	public class CustomTypeDescriptorWrapper<T> : ICustomTypeDescriptor
	{
		private readonly T _item;
		private readonly string _name;

		public CustomTypeDescriptorWrapper(T item, string name)
		{
			_item = item;
			_name = name;
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(typeof(T));
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(typeof(T));
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return _name;
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(typeof(T));
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(typeof(T));
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(typeof(T));
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(typeof(T), editorBaseType);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(typeof(T), attributes);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(typeof(T));
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(typeof(T), attributes);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return TypeDescriptor.GetProperties(typeof(T));
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return _item;
		}
	}
}
