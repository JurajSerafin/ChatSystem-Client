using System;
using ChatSystem.Client.Core.Domain.Chat;
using ChatSystem.Client.Core.Domain.Message;
using ChatSystem.Client.Core.Domain.User;
using ChatSystem.Client.Core.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Client.Core.Interfaces.Database;

/// <summary>
/// Represents the local Entity Framework Core database context responsible for caching and managing application data.
/// </summary>
/// <remarks>
/// This context acts as a local-first offline storage engine for the chat application. It manages the lifecycle of local cache elements 
/// including session tokens, users, active chat conversations, message participants, etc.
/// <para>
/// <b>Key Characteristics:</b>
/// <list type="bullet">
///     <item>
///         <description>
///             <b>DDD Value Conversion:</b> Converts strongly-typed domain identifiers (like <see cref="UserId"/>, <see cref="ChatId"/>, and <see cref="MessageId"/>) 
///             to standard primitives (<see cref="Guid"/> or <see cref="string"/>) for relational database persistence.
///         </description>
///     </item>
///     <item>
///         <description>
///             <b>Performance Indexes:</b> Implements specific descending indexes on chronological columns (<see cref="CachedMessage.CreatedAt"/> and <see cref="CachedChat.LastActivityAt"/>) 
///             to ensure high-performance pagination and sorting.
///         </description>
///     </item>
/// </list>
/// </para>
/// </remarks>
/// <param name="options">The database context options configuring provider settings, connection strings, and behavior.</param>
internal sealed class ChatSystemLocalDbContext(
    DbContextOptions<ChatSystemLocalDbContext> options) : DbContext(options) {

    /// <summary>
    /// Gets or sets the local single row data table containing cached data about the logged-in user.
    /// </summary>
    public DbSet<CachedIdentity> Identity { get; set; }

    /// <summary>
    /// Gets or sets the local data table containing cached profiles of users that the logged-in user has been recently interacting with.
    /// </summary>
    public DbSet<CachedUser> Users { get; set; }

    /// <summary>
    /// Gets or sets the local data table containing metadata for active chat rooms or direct message channels.
    /// </summary>
    public DbSet<CachedChat> Chats { get; set; }

    /// <summary>
    /// Gets or sets the junction data table tracking membership and mapping users to respective chats.
    /// </summary>
    public DbSet<CachedChatParticipant> ChatParticipants { get; set; }

    /// <summary>
    /// Gets or sets the local log of chat messages stored for offline viewing and rapid local retrieval.
    /// </summary>
    public DbSet<CachedMessage> Messages { get; set; }

    /// <summary>
    /// Overrides default model building configurations to enforce custom mappings, primary keys, value converters, relationships, and performance indexes.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the schema for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        ModelPrimaryKeys(modelBuilder);

        ModelValueConverters(modelBuilder);

        ModelRelationships(modelBuilder);

        ModelIndices(modelBuilder);
    }

    /// <summary>
    /// Configures the unique primary keys for each database entity.
    /// </summary>
    /// <param name="modelBuilder">The builder used to apply database key metadata.</param>
    private void ModelPrimaryKeys(ModelBuilder modelBuilder) {
        modelBuilder.Entity<CachedIdentity>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<CachedUser>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<CachedChat>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<CachedChatParticipant>()
            .HasKey(cp => new { cp.UserId, cp.ChatId });

        modelBuilder.Entity<CachedMessage>()
            .HasKey(m => m.Id);
    }

    /// <summary>
    /// Registers custom conversions for strongly-typed value object identifiers, bridging standard domain types with relational primitives.
    /// </summary>
    /// <param name="modelBuilder">The builder used to define custom value mapping strategies.</param>
    private void ModelValueConverters(ModelBuilder modelBuilder) {

        modelBuilder.Entity<CachedIdentity>()
            .Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                guid => UserId.Create(guid)
            );

        modelBuilder.Entity<CachedUser>()
            .Property(u => u.Id)
            .HasConversion(
                u => u.Value,
                guid => UserId.Create(guid)
            );

        modelBuilder.Entity<CachedChat>()
            .Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                guid => ChatId.Create(guid)
            );

        modelBuilder.Entity<CachedChat>()
            .Property(c => c.LastMessageId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value.ToString() : null,
                strGuid => strGuid == null ? null : MessageId.Create(new Guid(strGuid))
            );

        modelBuilder.Entity<CachedChatParticipant>()
            .Property(cp => cp.UserId)
            .HasConversion(
                userId => userId.Value,
                guid => UserId.Create(guid)
             );

        modelBuilder.Entity<CachedChatParticipant>()
            .Property(cp => cp.ChatId)
            .HasConversion(
                chatId => chatId.Value,
                guid => ChatId.Create(guid)
            );

        modelBuilder.Entity<CachedMessage>()
            .Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                guid => MessageId.Create(guid)
            );

        modelBuilder.Entity<CachedMessage>()
            .Property(m => m.SenderId)
            .HasConversion(
                id => id.Value,
                guid => UserId.Create(guid)
            );

        modelBuilder.Entity<CachedMessage>()
            .Property(m => m.ChatId)
            .HasConversion(
                id => id.Value,
                guid => ChatId.Create(guid)
            );
    }

    /// <summary>
    /// Configures navigational properties, foreign keys, and referential constraints governing relationships between cached objects.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct database relationships.</param>
    private void ModelRelationships(ModelBuilder modelBuilder) {
        // Chat -> Many Participants
        modelBuilder.Entity<CachedChatParticipant>()
            .HasOne(cp => cp.Chat)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ChatId);

        // User -> Many Participant Associations
        modelBuilder.Entity<CachedChatParticipant>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId);
    }

    /// <summary>
    /// Declares database indices inside local database tables.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct optimization indices.</param>
    private void ModelIndices(ModelBuilder modelBuilder) {
        // Speeds up grouping and filtering queries on specific channels
        modelBuilder.Entity<CachedMessage>()
            .HasIndex(m => m.ChatId);

        // Optimizes sorting messages chronologically (such as scrolling through chat history)
        modelBuilder.Entity<CachedMessage>()
            .HasIndex(m => m.CreatedAt)
            .IsDescending(true);

        // Optimizes sorting chats by the most recent interactions (such as the main feed layout)
        modelBuilder.Entity<CachedChat>()
            .HasIndex(c => c.LastActivityAt)
            .IsDescending(true);

        // Speeds up standard user queries looking up accounts alphabetically by login credentials
        modelBuilder.Entity<CachedUser>()
            .HasIndex(u => u.Login)
            .IsDescending(false);

        // Speeds up exact match lookups on unique system user tags
        modelBuilder.Entity<CachedUser>()
            .HasIndex(u => u.Tag);
    }
}