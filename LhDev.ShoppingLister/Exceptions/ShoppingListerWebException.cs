namespace LhDev.ShoppingLister.Exceptions;

/// <summary>
/// A custom exception type created to facilitate clean and simple error handling.
/// </summary>
/// <remarks>
/// Designed to making exception throwing and handling easier. There are static methods that generate the required exception based
/// on given parameters, if any. This is done to allow easy reuse of various exceptions, and to allow the exception handler pipeline
/// to easily identify our own exception type, which are designed to provide the all the right information about the error.
/// </remarks>
public class ShoppingListerWebException : ShoppingListerException
{
    public int StatusCode { get; init; }


    #region Constructors

    protected ShoppingListerWebException(string? message) : this(message, null)
    {
    }

    protected ShoppingListerWebException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    #endregion


    #region Test exceptions

    public static ShoppingListerWebException TestException()
        => new("This is a test ShoppingListerException type.") { StatusCode = StatusCodes.Status401Unauthorized };

    #endregion


    #region Authorisation and authentication

    public static ShoppingListerWebException BadCredentials()
        => new("Username or password is incorrect.") { StatusCode = StatusCodes.Status401Unauthorized };

    public static ShoppingListerWebException ApiUserIdNotFound(int id)
        => new($"User with id number '{id}' not found.") { StatusCode = StatusCodes.Status404NotFound };

    public static ShoppingListerWebException ApiListIdNotFound(int id)
        => new($"List with id number '{id}' not found.") { StatusCode = StatusCodes.Status404NotFound };

    public static ShoppingListerWebException ApiItemIdNotFound(int id)
        => new($"Item with id number '{id}' not found.") { StatusCode = StatusCodes.Status404NotFound };

    public static ShoppingListerWebException ApiUsernameNotFound(string username)
        => new($"Username '{username}' not found.") { StatusCode = StatusCodes.Status404NotFound };

    public static ShoppingListerWebException UserOrPasswordNotFound()
        => new("Username or password is incorrect.") { StatusCode = StatusCodes.Status401Unauthorized };

    public static ShoppingListerWebException UserNotVerified()
        => new("This account is not verified.") { StatusCode = StatusCodes.Status401Unauthorized };

    public static ShoppingListerWebException UserNoApi()
        => new("This account does not have API access.") { StatusCode = StatusCodes.Status401Unauthorized };

    public static ShoppingListerWebException UserBanned()
        => new("This account is banned.") { StatusCode = StatusCodes.Status401Unauthorized };

    public static ShoppingListerWebException UserExists(string username)
        => new($"The username '{username}' already exists.") { StatusCode = StatusCodes.Status400BadRequest };

    public static ShoppingListerWebException EmailExists(string email)
        => new($"The email '{email}' is already in use.") { StatusCode = StatusCodes.Status400BadRequest };

    public static ShoppingListerWebException CouldNotCreateUser()
        => new("Could not create user.") { StatusCode = StatusCodes.Status500InternalServerError };

    public static ShoppingListerWebException CouldNotCreatePassword()
        => new("Could not create password.") { StatusCode = StatusCodes.Status500InternalServerError };

    public static ShoppingListerWebException ApiListNotFound(string listName)
        => new($"List '{listName}' not found.") { StatusCode = StatusCodes.Status404NotFound };

    public static ShoppingListerWebException ListExists(string name, int userId)
        => new($"The list '{name}' already exists for user ID {userId}.") { StatusCode = StatusCodes.Status400BadRequest };

    public static ShoppingListerWebException CouldNotCreateList()
        => new("Could not create list.") { StatusCode = StatusCodes.Status500InternalServerError };

    public static ShoppingListerWebException ItemExists(string name, int listId)
        => new($"The item '{name}' already exists for list ID {listId}.") { StatusCode = StatusCodes.Status400BadRequest };

    public static ShoppingListerWebException CouldNotCreateItem()
        => new("Could not create item.") { StatusCode = StatusCodes.Status500InternalServerError };

    public static ShoppingListerWebException CantDeleteItemInWorkingList()
        => new("Could not delete item as it is in use by the list.") { StatusCode = StatusCodes.Status400BadRequest };

    #endregion


    #region User issues

    public static ShoppingListerWebException CouldNotUpdateUser(int id, string? extraInfo = null) 
        => new($"Could not update user with ID {id}. {extraInfo}".Trim());

    public static ShoppingListerWebException CouldNotFindUserId(int id) 
        => new($"Could not find user with ID {id}.");

    #endregion



    #region Not Authorised

    public static ShoppingListerWebException NotAuthorised(string message = "You are not authorised to access this resource.")
        => new(message) { StatusCode = StatusCodes.Status403Forbidden };

    public static ShoppingListerWebException NotAuthorisedUserViewList(int userId, int listId)
        => new($"User ID {userId} is not permitted to view list ID {listId}.") { StatusCode = StatusCodes.Status403Forbidden };

    #endregion



    #region Should not occur!
    
    public static ShoppingListerWebException ShouldNotHappen(string message = "This exception should not be thrown!")
        => new(message) { StatusCode = StatusCodes.Status500InternalServerError };
    
    public static ShoppingListerWebException ShouldNotHappenMissingUserContext()
        => new("The User object is missing from the HTTP context.") { StatusCode = StatusCodes.Status500InternalServerError };

    #endregion



    #region WebSocket

    public static ShoppingListerWebException ExpectingWebSocketRequest()
        => new("This endpoint is for WebSocket requests.") { StatusCode = StatusCodes.Status400BadRequest };

    #endregion
}