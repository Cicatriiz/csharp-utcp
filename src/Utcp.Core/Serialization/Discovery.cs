// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Serialization;

using Utcp.Core.Interfaces;
using Utcp.Core.Protocols;

public static class Discovery
{
	public static void RegisterProtocol(string type, ICommunicationProtocol protocol, bool overrideExisting = false)
	{
		ProtocolRegistry.Register(type, protocol, overrideExisting);
	}

	public static void RegisterCallTemplate(string discriminator, Type callTemplateType)
	{
		PolymorphicRegistry.RegisterCallTemplateDerivedType(discriminator, callTemplateType);
	}

	public static void RegisterAuth(string discriminator, Type authType)
	{
		PolymorphicRegistry.RegisterAuthDerivedType(discriminator, authType);
	}

	public static void RegisterSerializer<T>(Interfaces.IUtcpSerializer<T> serializer, bool overrideExisting = false)
	{
		SerializerRegistry.Register(serializer, overrideExisting);
	}
}
