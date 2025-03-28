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
                throw new RepositoryException("Failed to retrieve users from the database.", ex);
            }
        }

        public User? GetUserById(int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@user_id", userId)
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
                    new SqlParameter("@email", user.Email),
                    new SqlParameter("@username", user.Username),
                    new SqlParameter("@developer", user.IsDeveloper)
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
                    new SqlParameter("@hashed_password", user.Password),
                    new SqlParameter("@developer", user.IsDeveloper),
                    new SqlParameter("@created_at", user.CreatedAt)
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
                    new SqlParameter("@user_Id", userId)
                };

                _dataLink.ExecuteNonQuery("DeleteUser", parameters);
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException($"Failed to delete user with ID {userId}.", ex);
            }
        }

        public User? VerifyCredentials(string emailOrUsername, string hashedPassword)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmailOrUsername", emailOrUsername),
                    new SqlParameter("@Password", hashedPassword)
                };

                var dataTable = _dataLink.ExecuteReader("UserLogin", parameters);
                
                if (dataTable.Rows.Count > 0)
                {
                    return MapDataRowToUser(dataTable.Rows[0]);
                }
                
                return null;
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException("Failed to login user.", ex);
            }
        }

        public string CheckUserExists(string email, string username)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@email", email),
                    new SqlParameter("@username", username)
                };

                var dataTable = _dataLink.ExecuteReader("CheckUserExists", parameters);
                if (dataTable.Rows.Count > 0)
                {
                    return dataTable.Rows[0]["ErrorType"]?.ToString();
                }
                return null;
            }
            catch (DatabaseOperationException ex)
            {
                throw new RepositoryException("Failed to check if user exists.", ex);
            }
        }

        private static List<User> MapDataTableToUsers(DataTable dataTable)
        {
            return dataTable.AsEnumerable()
                .Select(MapDataRowToUser)
                .ToList();
        }

        private static User? MapDataRowToUser(DataRow row)
        {
            if (row["user_id"] == DBNull.Value || 
                row["email"] == DBNull.Value || 
                row["username"] == DBNull.Value)
            {
                return null;
            }

            return new User
            {
                UserId = Convert.ToInt32(row["user_id"]),
                Email = row["email"].ToString(),
                Username = row["username"].ToString(),
                IsDeveloper = row["developer"] != DBNull.Value ? Convert.ToBoolean(row["developer"]) : false,
                CreatedAt = row["created_at"] != DBNull.Value ? Convert.ToDateTime(row["created_at"]) : DateTime.MinValue,
                LastLogin = row["last_login"] != DBNull.Value ? row["last_login"] as DateTime? : null
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