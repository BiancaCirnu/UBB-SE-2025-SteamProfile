﻿using SteamProfile.Data;
using SteamProfile.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamProfile.Repositories
{
    public sealed class UsersRepository
    {
        private readonly DataLink _dataLink;

        public UsersRepository(DataLink datalink)
        {
            _dataLink = datalink ?? throw new ArgumentNullException(nameof(datalink));
        }

        public List<User> GetAllUsers()
        {
            try
            {
                var dataTable = _dataLink.ExecuteReader("GetAllUsers");
                return MapDataTableToUsers(dataTable);
            }
            catch (DatabaseOperationException ex)
            {
                // Log the error or handle it appropriately
                throw new RepositoryException("Failed to retrieve users from the database.", ex);
            }
        }

        public User? GetUserById(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId)
                };

                var dataTable = _dataLink.ExecuteReader("GetUserById", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUser(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException($"Failed to retrieve user with ID {userId} from the database.", ex);
            }
        }

        public User UpdateUser(User user)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", user.UserId),
                    new SqlParameter("@email", (object?)user.Email ?? DBNull.Value),
                    new SqlParameter("@username", (object?)user.Username ?? DBNull.Value),
                    new SqlParameter("@profile_picture", (object?)user.ProfilePicture ?? DBNull.Value),
                    new SqlParameter("@description", (object?)user.Description ?? DBNull.Value),
                    new SqlParameter("@developer", (object?)user.IsDeveloper ?? DBNull.Value)
                };

                var dataTable = _dataLink.ExecuteReader("UpdateUser", parameters);
                if (dataTable.Rows.Count == 0)
                {
                    throw new RepositoryException($"User with ID {user.UserId} not found.");
                }

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException($"Failed to update user with ID {user.UserId}.", ex);
            }
        }

        public User CreateUser(User user)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@username", user.Username),
                    new SqlParameter("@email", user.Email),
                    new SqlParameter("@password", user.Password),
                    new SqlParameter("@description", (object?)user.Description ?? DBNull.Value),
                    new SqlParameter("@developer", user.IsDeveloper),
                };

                var dataTable = _dataLink.ExecuteReader("CreateUser", parameters);
                if (dataTable.Rows.Count == 0)
                {
                    throw new RepositoryException("Failed to create user.");
                }

                return MapDataRowToUser(dataTable.Rows[0]);
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException("Failed to create user.", ex);
            }
        }

        public void DeleteUser(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId)
                };

                _dataLink.ExecuteNonQuery("DeleteUser", parameters);
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException($"Failed to delete user with ID {userId}.", ex);
            }
        }

        public User? GetUserByEmail(string email)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", email)
                };

                var dataTable = _dataLink.ExecuteReader("GetUserByEmail", parameters);
                return dataTable.Rows.Count > 0 ? MapDataRowToUser(dataTable.Rows[0]) : null;
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException($"Failed to retrieve user with email {email}.", ex);
            }
        }

        public void StoreResetCode(int userId, string resetCode)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@resetCode", resetCode),
                    new SqlParameter("@expirationTime", DateTime.UtcNow.AddHours(1))
                };

                _dataLink.ExecuteNonQuery("StorePasswordResetCode", parameters);
                Console.WriteLine(parameters);
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException($"Failed to store reset code for user {userId}.", ex);
            }
        }

        public bool VerifyResetCode(string email, string resetCode)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", email),
                    new SqlParameter("@resetCode", resetCode)
                };

                var result = _dataLink.ExecuteScalar<int>("VerifyResetCode", parameters);
                Console.WriteLine(result);
                return result == 1;
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException("Failed to verify reset code.", ex);
            }
        }

        public bool ResetPassword(string email, string resetCode, string hashedPassword)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", email),
                    new SqlParameter("@resetCode", resetCode),
                    new SqlParameter("@newPassword", hashedPassword)
                };

                var result = _dataLink.ExecuteScalar<int>("ResetPassword", parameters);
                return result == 1;
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException("Failed to reset password.", ex);
            }
        }

        private static List<User> MapDataTableToUsers(DataTable dataTable)
        {
            return dataTable.AsEnumerable()
                .Select(MapDataRowToUser)
                .ToList();
        }

        private static User MapDataRowToUser(DataRow row)
        {
            return new User
            {
                UserId = Convert.ToInt32(row["user_id"]),
                Email = row["email"].ToString() ?? string.Empty,
                Username = row["username"].ToString() ?? string.Empty,
                ProfilePicture = row["profile_picture"] as string,
                Description = row["description"] as string,
                IsDeveloper = Convert.ToBoolean(row["developer"]),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                LastLogin = row["last_login"] as DateTime?
            };
        }
    }

    public class RepositoryException : Exception
    {
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}