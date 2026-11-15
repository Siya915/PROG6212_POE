using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
namespace RaceDay.Mvc.Services;
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
}
public class ApiClient
{
    private const string ApiCookieKey="RaceDay.Api.Cookie";
    private readonly HttpClient _http; private readonly IHttpContextAccessor _accessor;
    public ApiClient(HttpClient http,IHttpContextAccessor accessor){_http=http;_accessor=accessor;}

    public Task<ApiResponse<T>> GetAsync<T>(string path)=>SendAsync<T>(HttpMethod.Get,path);
    public Task<ApiResponse<TOut>> PostAsync<TIn,TOut>(string path,TIn body)=>SendAsync<TOut>(HttpMethod.Post,path,JsonContent.Create(body));
    public Task<ApiResponse<TOut>> PutAsync<TIn,TOut>(string path,TIn body)=>SendAsync<TOut>(HttpMethod.Put,path,JsonContent.Create(body));
    public Task<ApiResponse<bool>> DeleteAsync(string path)=>SendAsync<bool>(HttpMethod.Delete,path);

    public async Task<ApiResponse<TOut>> UploadAsync<TOut>(string path,IFormFile file)
    {
        using var content=new MultipartFormDataContent();
        await using var stream=file.OpenReadStream();
        var fileContent=new StreamContent(stream); fileContent.Headers.ContentType=MediaTypeHeaderValue.Parse(file.ContentType);
        content.Add(fileContent,"file",Path.GetFileName(file.FileName));
        return await SendAsync<TOut>(HttpMethod.Post,path,content);
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method,string path,HttpContent? content=null)
    {
        using var request=new HttpRequestMessage(method,path); request.Content=content;
        var context=_accessor.HttpContext;
        var apiCookie=context?.Session.GetString(ApiCookieKey);
        if(!string.IsNullOrWhiteSpace(apiCookie)) request.Headers.Add("Cookie",apiCookie);
        try
        {
            using var response=await _http.SendAsync(request);
            if(response.Headers.TryGetValues("Set-Cookie",out var cookies))
            {
                var cookie=cookies.FirstOrDefault(x=>x.StartsWith(".RaceDay.Api.Session=",StringComparison.Ordinal));
                if(cookie!=null)
                {
                    var pair=cookie.Split(';',2)[0];
                    if(pair.EndsWith("=")) context?.Session.Remove(ApiCookieKey);
                    else context?.Session.SetString(ApiCookieKey,pair);
                }
            }
            T? data=default;
            if(response.IsSuccessStatusCode && response.Content.Headers.ContentLength!=0)
            {
                try { data=await response.Content.ReadFromJsonAsync<T>(); } catch { data=default; }
            }
            string? error=null;
            if(!response.IsSuccessStatusCode)
            {
                var raw=await response.Content.ReadAsStringAsync();
                try
                {
                    using var json=System.Text.Json.JsonDocument.Parse(raw);
                    var root=json.RootElement;
                    if(root.TryGetProperty("message",out var message)) error=message.GetString();
                    else if(root.TryGetProperty("errors",out var errors) && errors.ValueKind==System.Text.Json.JsonValueKind.Object)
                    {
                        var messages=errors.EnumerateObject().SelectMany(x=>x.Value.ValueKind==System.Text.Json.JsonValueKind.Array?x.Value.EnumerateArray().Select(v=>v.GetString()??""):new[]{x.Value.ToString()}).Where(x=>!string.IsNullOrWhiteSpace(x));
                        error=string.Join(" ",messages);
                    }
                    else if(root.TryGetProperty("title",out var title)) error=title.GetString();
                }
                catch { error=null; }
                if(string.IsNullOrWhiteSpace(error))
                {
                    error=response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => "Please log in again.",
                        HttpStatusCode.Forbidden => "You do not have permission to perform this action.",
                        HttpStatusCode.NotFound => "The requested item was not found.",
                        HttpStatusCode.Conflict => "The request conflicts with the current event or enrolment details.",
                        _ => "The request could not be completed. Please check the information and try again."
                    };
                }
            }
            return new ApiResponse<T>{Success=response.IsSuccessStatusCode,StatusCode=response.StatusCode,Data=data,Error=error};
        }
        catch(HttpRequestException)
        {
            return new ApiResponse<T>{Success=false,StatusCode=HttpStatusCode.ServiceUnavailable,Error="The RaceDay API is not responding. Start RaceDay.Api and try again."};
        }
    }
}
