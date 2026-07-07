// JwtSettings lives in Patogh.Application.Configurations to avoid circular dependency.
// This alias lets Infrastructure code continue using the unqualified name JwtSettings.
global using JwtSettings = Patogh.Application.Configurations.JwtSettings;