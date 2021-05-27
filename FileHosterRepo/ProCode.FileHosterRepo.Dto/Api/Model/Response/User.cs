﻿using System;

namespace ProCode.FileHosterRepo.Api.Model.Response
{
    public class User
    {
        #region Constructor
        public User() { }
        #endregion

        #region Properties
        public int UserId { get; set; }
        //public string Email { get; set; }
        public string Nickname { get; set; }
        public DateTime? Created { get; set; }
        public Dal.Model.UserRole Role { get; set; }
        #endregion
    }
}