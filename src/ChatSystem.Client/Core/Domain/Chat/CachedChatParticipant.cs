using System.ComponentModel.DataAnnotations;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Core.Domain.Chat;


/// <summary>
/// Represents a junction entity that establishes and manages the cached many-to-many relationship 
/// between a user and a chat conversation in the local database.
/// </summary>
/// <remarks>
/// This entity acts as the relational bridge connecting a <see cref="CachedUser"/> to a <see cref="CachedChat"/>. 
/// Beyond forming the link, it carries contextual metadata specific to the relationship, such as the 
/// user's access level or position (<see cref="Role"/>) within that specific conversation.
/// <para>
/// <b>Database Schema:</b>
/// <list type="bullet">
///     <item>
///         <description>
///             Uses a composite primary key consisting of both <see cref="UserId"/> and <see cref="ChatId"/>.
///         </description>
///     </item>
///     <item>
///         <description>
///             Configures cascades and foreign key constraints within the EF Core model builder.
///         </description>
///     </item>
/// </list>
/// </para>
/// </remarks>
public class CachedChatParticipant {

    /// <summary>
    /// Gets or sets the ID of the participating user.
    /// </summary>
    /// <remarks>
    /// This forms the first half of the composite primary key and acts as the foreign key 
    /// linking to the <see cref="CachedUser"/> entity.
    /// </remarks>
    public UserId UserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the chat conversation.
    /// </summary>
    /// <remarks>
    /// This forms the second half of the composite primary key and acts as the foreign key 
    /// linking to the <see cref="CachedChat"/> entity.
    /// </remarks>
    public ChatId ChatId { get; set; }

    /// <summary>
    /// Gets or sets the privileges or status level designated to the user inside this chat room.
    /// </summary>
    /// <value>
    /// A non-nullable string value representing administrative permissions.
    /// </value>
    [MaxLength(32)]
    public string Role { get; set; } = null!;


    //--- Entity navigation

    /// <summary>
    /// Gets or sets the Entity Framework navigational property pointing to the cached details of the associated user.
    /// </summary>
    /// <remarks>
    /// This property is automatically mapped and loaded by the database engine utilizing the <see cref="UserId"/> foreign key relationship.
    /// </remarks>
    public CachedUser User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Entity Framework navigational property pointing to the cached metadata of the associated chat room.
    /// </summary>
    /// <remarks>
    /// This property is automatically mapped and loaded by the database engine utilizing the <see cref="ChatId"/> foreign key relationship.
    /// </remarks>
    public CachedChat Chat { get; set; } = null!;
}