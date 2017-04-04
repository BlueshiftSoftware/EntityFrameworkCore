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
            var identityUserType = FindGenericBaseType(userType, typeof(MongoDbIdentityUser<,,,,>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException($"User type does not derive from {typeof(MongoDbIdentityUser<,,,,>).FullName}.");
            }
            var identityRoleType = FindGenericBaseType(roleType, typeof(MongoDbIdentityRole<,>));
            if (identityRoleType == null)
            {
                throw new InvalidOperationException($"Role type does not derive from {typeof(MongoDbIdentityRole<,>).FullName}.");
            }

            var userStoreType = typeof(MongoDbUserStore<,,,,,,,>)
                    .MakeGenericType(
                        userType,
                        roleType,
                        contextType,
                        keyType ?? identityUserType.GenericTypeArguments[0],
                        identityUserType.GenericTypeArguments[1], //TClaim
                        identityUserType.GenericTypeArguments[2], //TUserRole
                        identityUserType.GenericTypeArguments[3], //TUserLogin
                        identityUserType.GenericTypeArguments[4]); //TUserToken
            var roleStoreType = typeof(MongoDbRoleStore<,,,>)
                    .MakeGenericType(
                        roleType,
                        contextType,
                        keyType ?? identityUserType.GenericTypeArguments[0],
                        identityRoleType.GenericTypeArguments[1]); //TClaim

            Func<IServiceProvider, object> getUserStore = provider => provider.GetRequiredService(userStoreType);
            Func<IServiceProvider, object> getRoleStore = provider => provider.GetRequiredService(roleStoreType);

            services.TryAddScoped(userStoreType);
            services.TryAddScoped(typeof(IQueryableUserStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserAuthenticationTokenStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserAuthenticatorKeyStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserClaimStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserEmailStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserLoginStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserLockoutStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserPasswordStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserRoleStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserPhoneNumberStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserSecurityStampStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserTwoFactorStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserTwoFactorRecoveryCodeStore<>).MakeGenericType(userType), getUserStore);
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), getUserStore);

            services.TryAddScoped(roleStoreType);
            services.TryAddScoped(typeof(IQueryableRoleStore<>).MakeGenericType(roleType), getRoleStore);
            services.TryAddScoped(typeof(IRoleClaimStore<>).MakeGenericType(roleType), getRoleStore);
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), getRoleStore);
        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType.GetTypeInfo();
            while (type.BaseType != null)
            {
                type = type.BaseType.GetTypeInfo();
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                {
                    return type;
                }
            }
            return null;
        }
    }
}