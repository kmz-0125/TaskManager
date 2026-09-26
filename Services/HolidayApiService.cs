using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Services
{
    public class HolidayApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public HolidayApiService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task FetchAndSaveHolidaysAsync()
        {
            bool hasData = await _context.Holidays.AnyAsync();

            if (hasData)
            {
                return;
            }

            // 指定したURLにGETリクエストを送り、返ってきた内容を、そのまま文字列として受け取る
            string url = "https://holidays-jp.github.io/api/v1/date.json";
            string json = await _httpClient.GetStringAsync(url); // json変数にはリストのような構造化されたデータが入っているわけではない
            // JSON文字列の変換(シリアライズ/デシリアライズ)
            var holidayDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            //変換に失敗した場合、nullを返す可能性があるためnullチェック
            if (holidayDict == null)
            {
                return;
            }

            foreach (var pair in holidayDict)
            {
                // pair.Key に、日付(文字列)が入っている
                // pair.Value に、祝日名が入っている
                DateTime parseDate = DateTime.Parse(pair.Key);

                var holiday = new Holiday
                {
                    Date = parseDate,
                    HolidayName = pair.Value,
                };

                _context.Holidays.Add(holiday);

            }
            await _context.SaveChangesAsync();
        }
    }
}