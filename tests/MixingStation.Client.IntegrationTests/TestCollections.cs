using Xunit;

namespace MixingStation.Client.IntegrationTests;

/// <summary>
/// Collection for tests that require active mixer connection.
/// These tests reuse existing connection and run BEFORE disconnect tests.
/// </summary>
[CollectionDefinition("ConnectionDependent", DisableParallelization = true)]
public class ConnectionDependentCollection
{
}

/// <summary>
/// Collection for tests that disconnect and break connection.
/// These tests run LAST to avoid breaking connection-dependent tests.
/// </summary>
[CollectionDefinition("DisconnectTests", DisableParallelization = true)]
public class DisconnectTestsCollection
{
}
