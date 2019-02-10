using System;
using System.Reflection;
using Blueshift.Identity.MongoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Provides extension methods to <see cref="IdentityBuilder"/> for adding MongoDB for EntityFramework stores.
    /// </summary>
    public static class IdentityEntityFrameworkMongoDbBuilderExtensions
    {
        private static readonly Type BaseUserType = typeof(MongoDbIdentityUser<,,,,>);
        private static readonly Type BaseRoleType = typeof(MongoDbIdentityRole<,>);
        private static readonly Type BaseStoreType = typeof(MongoDbUserStore<,,,,,,,>);

        /// <summary>
        /// Adds a MongoDB for Entity Framework implementation of identity information stores.
        /// </summary>
        /// <typeparam name="TContext">The Entity Framework database context to use.</typeparam>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddEntityFrameworkMongoDbStores<TContext>(this IdentityBuilder builder)
            where TContext : DbContext
        {
            AddMongoDbStores(builder.Services, builder.UserType, builder.RoleType, typeof(TContext));
            return builder;
        }

        /// <summary>
        /// Adds a default MongoDB for Entity Framework implementation of identity information stores.
        /// </summary>
        /// <typeparam name="TContext">The Entity Framework database context to use.</typeparam>
        /// <typeparam name="TKey">The type of the primary key used for the users and roles.</typeparam>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddEntityFrameworkDbStores<TContext, TKey>(this IdentityBuilder builder)
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            AddMongoDbStores(builder.Services, builder.UserType, builder.RoleType, typeof(TContext), typeof(TKey));
            return builder;
        }

        private static void AddMongoDbStores(IServiceCollection services, Type userType, Type roleType, Type contextType, Type keyType = null)
        {
            TypeInfo identityUserType = FindGenericBaseType(userType, BaseUserType);
            if (identityUserType == null)
            {
                throw new InvalidOperationException($"User type does not derive from {BaseUserType.FullName}.");
            }
            TypeInfo identityRoleType = FindGenericBaseType(roleType, BaseRoleType);
            if (identityRoleType == null)
            {
                throw new InvalidOperationException($"Role type does not derive from {BaseRoleType.FullName}.");
            }

            Type userStoreType = BaseStoreType
                    .MakeGenericType(
                        userType,
                        roleType,
                        contextType,
                        keyType ?? identityUserType.GenericTypeArguments[0],
                        identityUserType.GenericTypeArguments[1], //TClaim
                        identityUserType.GenericTypeArguments[2], //TUserRole
                        identityUserType.GenericTypeArguments[3], //TUserLogin
                        identityUserType.GenericTypeArguments[4]); //TUserToken
            Type roleStoreType = typeof(MongoDbRoleStore<,,,>)
                    .MakeGenericType(
                        roleType,
                        contextType,
                        keyType ?? identityUserType.GenericTypeArguments[0],
                        identityRoleType.GenericTypeArguments[1]); //TClaim

            object GetUserStore(IServiceProvider provider) => provider.GetRequiredService(userStoreType);
            object GetRoleStore(IServiceProvider provider) => provider.GetRequiredService(roleStoreType);

            services.TryAddScoped(userStoreType);
            services.TryAddScoped(typeof(IQueryableUserStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserAuthenticationTokenStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserAuthenticatorKeyStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserClaimStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserEmailStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserLoginStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserLockoutStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserPasswordStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserRoleStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserPhoneNumberStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserSecurityStampStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserTwoFactorStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserTwoFactorRecoveryCodeStore<>).MakeGenericType(userType), GetUserStore);
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), GetUserStore);

            services.TryAddScoped(roleStoreType);
            services.TryAddScoped(typeof(IQueryableRoleStore<>).MakeGenericType(roleType), GetRoleStore);
            services.TryAddScoped(typeof(IRoleClaimStore<>).MakeGenericType(roleType), GetRoleStore);
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), GetRoleStore);
        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType.GetTypeInfo();
            while (type != null)
            {
                type = type.GetTypeInfo();
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                {
                    return type;
                }

                type = type.BaseType.GetTypeInfo();
            }
            return null;
        }
    }
}