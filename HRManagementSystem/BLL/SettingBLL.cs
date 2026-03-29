using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRManagementSystem.BLL
{
    public class SettingBLL : BaseBLL<Setting>
    {
        private readonly SettingDAL _settingDAL;

        public SettingBLL() : base(new SettingDAL())
        {
            _settingDAL = (SettingDAL)_baseDAL;
        }

        public List<Setting> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _settingDAL.GetAll().ToList();
            }

            return _settingDAL.GetAll()
                .Where(s =>
                    s.SettingKey.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || s.SettingValue.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrEmpty(s.Description) && s.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(s.Status) && s.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
