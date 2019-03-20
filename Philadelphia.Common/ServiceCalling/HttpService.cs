using System;

namespace Philadelphia.Common {
    // REVIEW: .NET says that we should end attribute names with Attribute suffix, do we care?
	[AttributeUsage(AttributeTargets.Interface)]
	public class HttpService : Attribute {
	}
}
