using Microsoft.AspNetCore.Http;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a response from an API.
/// </summary>
/// <typeparam name="T">The type of the data contained in the response.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class.
    /// </summary>
    /// <param name="data">The data to be included in the response.</param>
    /// <param name="message">An optional message to be included in the response.</param>
    public ApiResponse(T data, string? message = null)
    {
        Results = data;
        Message = message;
    }

    /// <summary>
    /// Gets or sets the results of the API response.
    /// </summary>
    public T Results { get; set; }

    /// <summary>
    /// Gets or sets an optional message to be included in the response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Creates a successful <see cref="ApiResponse{T}"/> with the provided data and an optional message.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">An optional message to include in the response.</param>
    /// /// <returns>An <see cref="ApiResponse{T}"/> containing the data and message.</returns>
    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>(data, message);
    }
}
