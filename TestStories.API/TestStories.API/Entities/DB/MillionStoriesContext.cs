using System;
using Microsoft.EntityFrameworkCore;

namespace MillionStories.API.Entities.DB
{
    public partial class MillionStoriesContext : DbContext
    {
        public MillionStoriesContext()
        {
        }

        public MillionStoriesContext(DbContextOptions<MillionStoriesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EditorPicks> EditorPicks { get; set; }
        public virtual DbSet<EngagementType> EngagementType { get; set; }
        public virtual DbSet<Experiment> Experiment { get; set; }
        public virtual DbSet<ExperimentMedia> ExperimentMedia { get; set; }
        public virtual DbSet<ExperimentStatus> ExperimentStatus { get; set; }
        public virtual DbSet<ExperimentType> ExperimentType { get; set; }
        public virtual DbSet<Favorites> Favorites { get; set; }
        public virtual DbSet<FlywaySchemaHistory> FlywaySchemaHistory { get; set; }
        public virtual DbSet<Media> Media { get; set; }
        public virtual DbSet<MediaSrt> MediaSrt { get; set; }
        public virtual DbSet<MediaStatus> MediaStatus { get; set; }
        public virtual DbSet<MediaTag> MediaTag { get; set; }
        public virtual DbSet<MediaTopic> MediaTopic { get; set; }
        public virtual DbSet<MediaType> MediaType { get; set; }
        public virtual DbSet<Partner> Partner { get; set; }
        public virtual DbSet<PartnerMedia> PartnerMedia { get; set; }
        public virtual DbSet<PartnerPartnerType> PartnerPartnerType { get; set; }
        public virtual DbSet<PartnerType> PartnerType { get; set; }
        public virtual DbSet<Playlist> Playlist { get; set; }
        public virtual DbSet<PlaylistMedia> PlaylistMedia { get; set; }
        public virtual DbSet<Series> Series { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<SubscriptionSeries> SubscriptionSeries { get; set; }
        public virtual DbSet<SubscriptionTopic> SubscriptionTopic { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<Tool> Tool { get; set; }
        public virtual DbSet<ToolMedia> ToolMedia { get; set; }
        public virtual DbSet<ToolSeries> ToolSeries { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserStatus> UserStatus { get; set; }
        public virtual DbSet<UserType> UserType { get; set; }
        public virtual DbSet<WatchHistory> WatchHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<EditorPicks>(entity =>
            {
                entity.HasIndex(e => e.Title)
                    .HasDatabaseName("UQ_dboEditorPicks_title")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.EmbeddedCode)
                    .IsRequired()
                    .HasColumnName("embedded_code");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<EngagementType>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboEngagementType_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Experiment>(entity =>
            {
                entity.HasIndex(e => e.EngagementtypeId)
                    .HasDatabaseName("IX_dboExperiment_engagementtype");

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboExperiment_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");

                entity.Property(e => e.EndDateUtc)
                    .HasColumnName("end_date_utc")
                    .HasColumnType("datetime");

                entity.Property(e => e.EngagementtypeId).HasColumnName("engagementtype_id");

                entity.Property(e => e.ExperimentstatusId).HasColumnName("experimentstatus_id");

                entity.Property(e => e.ExperimenttypeId).HasColumnName("experimenttype_id");

                entity.Property(e => e.Goal).HasColumnName("goal");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.StartDateUtc)
                    .HasColumnName("start_date_utc")
                    .HasColumnType("datetime");

                entity.Property(e => e.VideoPlays).HasColumnName("video_plays");

                entity.HasOne(d => d.CreatedUser)
                    .WithMany(p => p.Experiment)
                    .HasForeignKey(d => d.CreatedUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboExperiment_dboUser");

                entity.HasOne(d => d.Engagementtype)
                    .WithMany(p => p.Experiment)
                    .HasForeignKey(d => d.EngagementtypeId)
                    .HasConstraintName("FK_dboExperiment_dboEngagementType");

                entity.HasOne(d => d.Experimentstatus)
                    .WithMany(p => p.Experiment)
                    .HasForeignKey(d => d.ExperimentstatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboExperiment_dboExperimentStatus");

                entity.HasOne(d => d.Experimenttype)
                    .WithMany(p => p.Experiment)
                    .HasForeignKey(d => d.ExperimenttypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboExperiment_dboExperimentType");
            });

            modelBuilder.Entity<ExperimentMedia>(entity =>
            {
                entity.ToTable("Experiment_Media");

                entity.HasIndex(e => new { e.ExperimentId, e.TitleImage, e.MediaId })
                    .HasDatabaseName("IX_dboExperiment_Media_media");

                entity.HasIndex(e => new { e.TitleImage, e.ExperimentId, e.MediaId })
                    .HasDatabaseName("UQ_dboExperiment_Media")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ExperimentId).HasColumnName("experiment_id");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.TitleImage)
                    .HasColumnName("title_image")
                    .HasMaxLength(200);

                entity.Property(e => e.VideoPlayCount).HasColumnName("video_play_count");

                entity.HasOne(d => d.Experiment)
                    .WithMany(p => p.ExperimentMedia)
                    .HasForeignKey(d => d.ExperimentId)
                    .HasConstraintName("FK_dboExperiment_Media_dboExperiment");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.ExperimentMedia)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboExperiment_Media_dboMedia");
            });

            modelBuilder.Entity<ExperimentStatus>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboExperimentStatus_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ExperimentType>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboExperimentType_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Favorites>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.MediaId })
                    .HasDatabaseName("UQ_dboFavorites")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboFavorites_dboMedia");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dboFavorites_dboUser");
            });

            modelBuilder.Entity<FlywaySchemaHistory>(entity =>
            {
                entity.HasKey(e => e.InstalledRank)
                    .HasName("flyway_schema_history_pk");

                entity.ToTable("flyway_schema_history");

                entity.HasIndex(e => e.Success)
                    .HasDatabaseName("flyway_schema_history_s_idx");

                entity.Property(e => e.InstalledRank)
                    .HasColumnName("installed_rank")
                    .ValueGeneratedNever();

                entity.Property(e => e.Checksum).HasColumnName("checksum");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(200);

                entity.Property(e => e.ExecutionTime).HasColumnName("execution_time");

                entity.Property(e => e.InstalledBy)
                    .IsRequired()
                    .HasColumnName("installed_by")
                    .HasMaxLength(100);

                entity.Property(e => e.InstalledOn)
                    .HasColumnName("installed_on")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Script)
                    .IsRequired()
                    .HasColumnName("script")
                    .HasMaxLength(1000);

                entity.Property(e => e.Success).HasColumnName("success");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(20);

                entity.Property(e => e.Version)
                    .HasColumnName("version")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasIndex(e => e.MediatypeId)
                    .HasDatabaseName("IX_dboMedia_mediatype");

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboMedia_name")
                    .IsUnique();

                entity.HasIndex(e => e.PublishUserId)
                    .HasDatabaseName("IX_dboMedia_publishuser");

                entity.HasIndex(e => e.SeriesId)
                    .HasDatabaseName("IX_dboMedia_series");

                entity.HasIndex(e => e.SourceId)
                    .HasDatabaseName("IX_dboMedia_source");

                entity.HasIndex(e => e.TopicId)
                    .HasDatabaseName("IX_dboMedia_topic");

                entity.HasIndex(e => e.UploadUserId)
                    .HasDatabaseName("IX_dboMedia_uploaduser");

                entity.HasIndex(e => new { e.Name, e.Description, e.MediastatusId, e.MediatypeId, e.TopicId, e.SeriesId, e.SourceId, e.UploadUserId, e.PublishUserId, e.DatePublishedUtc })
                    .HasDatabaseName("IX_dboMedia_date_published_utc");

                entity.HasIndex(e => new { e.Name, e.Description, e.MediatypeId, e.TopicId, e.SeriesId, e.SourceId, e.UploadUserId, e.PublishUserId, e.MediastatusId, e.DatePublishedUtc })
                    .HasDatabaseName("IX_dboMedia_mediastatus");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ActiveFromUtc)
                    .HasColumnName("active_from_utc")
                    .HasColumnType("datetime");

                entity.Property(e => e.ActiveToUtc)
                    .HasColumnName("active_to_utc")
                    .HasColumnType("datetime");

                entity.Property(e => e.DateCreatedUtc)
                    .HasColumnName("date_created_utc")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.DatePublishedUtc)
                    .HasColumnName("date_published_utc")
                    .HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(200)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.EmbeddedCode)
                    .IsRequired()
                    .HasColumnName("embedded_code")
                    .HasMaxLength(500)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FeaturedImage)
                    .IsRequired()
                    .HasColumnName("featured_image")
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FeaturedImageMetadata).HasColumnName("featured_image_metadata");

                entity.Property(e => e.IsPrivate).HasColumnName("is_private");

                entity.Property(e => e.IsSharingAllowed)
                    .IsRequired()
                    .HasColumnName("is_sharing_allowed")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.MediastatusId).HasColumnName("mediastatus_id");

                entity.Property(e => e.MediatypeId).HasColumnName("mediatype_id");

                entity.Property(e => e.Metadata).HasColumnName("metadata");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(90);

                entity.Property(e => e.PublishUserId).HasColumnName("publish_user_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.SeriesId).HasColumnName("series_id");

                entity.Property(e => e.SourceId).HasColumnName("source_id");

                entity.Property(e => e.SrtFile)
                    .HasColumnName("srt_file")
                    .HasMaxLength(256);

                entity.Property(e => e.SrtFileMetadata).HasColumnName("srt_file_metadata");

                entity.Property(e => e.Thumbnail)
                    .IsRequired()
                    .HasColumnName("thumbnail")
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TopicId).HasColumnName("topic_id");

                entity.Property(e => e.UploadUserId).HasColumnName("upload_user_id");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url")
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.Mediastatus)
                    .WithMany(p => p.Media)
                    .HasForeignKey(d => d.MediastatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboMedia_dboMediaStatus");

                entity.HasOne(d => d.Mediatype)
                    .WithMany(p => p.Media)
                    .HasForeignKey(d => d.MediatypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboMedia_dboMediaType");

                entity.HasOne(d => d.PublishUser)
                    .WithMany(p => p.MediaPublishUser)
                    .HasForeignKey(d => d.PublishUserId)
                    .HasConstraintName("FK_dboMedia_dboUser2");

                entity.HasOne(d => d.Series)
                    .WithMany(p => p.Media)
                    .HasForeignKey(d => d.SeriesId)
                    .HasConstraintName("FK_dboMedia_dboSeries");

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.Media)
                    .HasForeignKey(d => d.SourceId)
                    .HasConstraintName("FK_dboMedia_dboPartner");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.Media)
                    .HasForeignKey(d => d.TopicId)
                    .HasConstraintName("FK_dboMedia_dboTopic");

                entity.HasOne(d => d.UploadUser)
                    .WithMany(p => p.MediaUploadUser)
                    .HasForeignKey(d => d.UploadUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboMedia_dboUser1");
            });

            modelBuilder.Entity<MediaSrt>(entity =>
            {
                entity.HasIndex(e => e.MediaId)
                    .HasDatabaseName("IX_dboMediaSrt_media");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.File)
                    .IsRequired()
                    .HasColumnName("file")
                    .HasMaxLength(256);

                entity.Property(e => e.FileMetadata)
                    .IsRequired()
                    .HasColumnName("file_metadata");

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasColumnName("language")
                    .HasMaxLength(256);

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaSrt)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboMediaSrt_dboMedia");
            });

            modelBuilder.Entity<MediaStatus>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboMediaStatus_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<MediaTag>(entity =>
            {
                entity.ToTable("Media_Tag");

                entity.HasIndex(e => new { e.MediaId, e.TagId })
                    .HasDatabaseName("UQ_dboMedia_Tag")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.TagId).HasColumnName("tag_id");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaTag)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboMedia_Tag_dboMedia");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.MediaTag)
                    .HasForeignKey(d => d.TagId)
                    .HasConstraintName("FK_dboMedia_Tag_dboTag");
            });

            modelBuilder.Entity<MediaTopic>(entity =>
            {
                entity.ToTable("Media_Topic");

                entity.HasIndex(e => new { e.MediaId, e.TopicId })
                    .HasDatabaseName("UQ_dboMedia_Topic")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.TopicId).HasColumnName("topic_id");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaTopic)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboMedia_Topic_dboMedia");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.MediaTopic)
                    .HasForeignKey(d => d.TopicId)
                    .HasConstraintName("FK_dboMedia_Topic_dboTopic");
            });

            modelBuilder.Entity<MediaType>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboMediaType_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Partner>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboPartner_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateAddedUtc)
                    .HasColumnName("date_added_utc")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(200)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.IsArchived).HasColumnName("is_archived");

                entity.Property(e => e.Link).HasColumnName("link");

                entity.Property(e => e.Logo)
                    .IsRequired()
                    .HasColumnName("logo")
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.LogoMetadata).HasColumnName("logo_metadata");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(50);

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.ShowOnPartnerPage).HasColumnName("show_on_partner_page");
            });

            modelBuilder.Entity<PartnerMedia>(entity =>
            {
                entity.ToTable("Partner_Media");

                entity.HasIndex(e => new { e.PartnerId, e.MediaId })
                    .HasDatabaseName("UQ_dboPartner_Media")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(100);

                entity.Property(e => e.EndDateUtc)
                    .HasColumnName("end_date_utc")
                    .HasColumnType("datetime");

                entity.Property(e => e.IsExpired).HasColumnName("is_expired");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.PartnerId).HasColumnName("partner_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.StartDateUtc)
                    .HasColumnName("start_date_utc")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.PartnerMedia)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboPartner_Media_dboMedia");

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.PartnerMedia)
                    .HasForeignKey(d => d.PartnerId)
                    .HasConstraintName("FK_dboPartner_Media_dboPartner");
            });

            modelBuilder.Entity<PartnerPartnerType>(entity =>
            {
                entity.ToTable("Partner_PartnerType");

                entity.HasIndex(e => new { e.PartnerId, e.PartnertypeId })
                    .HasDatabaseName("UQ_dboPartner_PartnerType")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.PartnerId).HasColumnName("partner_id");

                entity.Property(e => e.PartnertypeId).HasColumnName("partnertype_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.PartnerPartnerType)
                    .HasForeignKey(d => d.PartnerId)
                    .HasConstraintName("FK_dboPartner_PartnerType_dboPartner");

                entity.HasOne(d => d.Partnertype)
                    .WithMany(p => p.PartnerPartnerType)
                    .HasForeignKey(d => d.PartnertypeId)
                    .HasConstraintName("FK_dboPartner_PartnerType_dboPartnerType");
            });

            modelBuilder.Entity<PartnerType>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboPartnerType_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_dboPlaylist_user");

                entity.HasIndex(e => new { e.Name, e.UserId })
                    .HasDatabaseName("UQ_dboPlaylist_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(48);

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Playlist)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dboPlaylist_dboUser");
            });

            modelBuilder.Entity<PlaylistMedia>(entity =>
            {
                entity.ToTable("Playlist_Media");

                entity.HasIndex(e => new { e.MediaId, e.PlaylistId, e.MediaSequence })
                    .HasDatabaseName("UQ_dboPlaylist_Media_playlist_mediasequence")
                    .IsUnique();

                entity.HasIndex(e => new { e.MediaSequence, e.PlaylistId, e.MediaId })
                    .HasDatabaseName("UQ_dboPlaylist_Media")
                    .IsUnique();

                entity.HasIndex(e => new { e.PlaylistId, e.MediaSequence, e.MediaId })
                    .HasDatabaseName("IX_dboPlaylist_Media_media");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.MediaSequence).HasColumnName("media_sequence");

                entity.Property(e => e.PlaylistId).HasColumnName("playlist_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.PlaylistMedia)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboPlaylist_Media_dboMedia");

                entity.HasOne(d => d.Playlist)
                    .WithMany(p => p.PlaylistMedia)
                    .HasForeignKey(d => d.PlaylistId)
                    .HasConstraintName("FK_dboPlaylist_Media_dboPlaylist");
            });

            modelBuilder.Entity<Series>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboSeries_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(400)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FeaturedImage)
                    .IsRequired()
                    .HasColumnName("featured_image")
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.FeaturedImageMetadata).HasColumnName("featured_image_metadata");

                entity.Property(e => e.HomepageBanner)
                    .HasColumnName("homepage_banner")
                    .HasMaxLength(256);

                entity.Property(e => e.HomepageBannerMetadata).HasColumnName("homepage_banner_metadata");

                entity.Property(e => e.Logo)
                    .IsRequired()
                    .HasColumnName("logo")
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.LogoMetadata).HasColumnName("logo_metadata");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(40);

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasIndex(e => new { e.Value, e.Name })
                    .HasDatabaseName("UQ_dboSetting_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(256);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasColumnType("nvarchar(max)");
            });

            modelBuilder.Entity<SubscriptionSeries>(entity =>
            {
                entity.ToTable("Subscription_Series");

                entity.HasIndex(e => new { e.UserId, e.SeriesId })
                    .HasDatabaseName("UQ_dboSubscription_Series")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.SeriesId).HasColumnName("series_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Series)
                    .WithMany(p => p.SubscriptionSeries)
                    .HasForeignKey(d => d.SeriesId)
                    .HasConstraintName("FK_dboSubscription_Series_dboSeries");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SubscriptionSeries)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dboSubscription_Series_dboUser");
            });

            modelBuilder.Entity<SubscriptionTopic>(entity =>
            {
                entity.ToTable("Subscription_Topic");

                entity.HasIndex(e => new { e.UserId, e.TopicId })
                    .HasDatabaseName("UQ_dboSubscription_Topic")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.TopicId).HasColumnName("topic_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.SubscriptionTopic)
                    .HasForeignKey(d => d.TopicId)
                    .HasConstraintName("FK_dboSubscription_Topic_dboTopic");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SubscriptionTopic)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dboSubscription_Topic_dboUser");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboTag_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(90);

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();
            });

            modelBuilder.Entity<Tool>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboTool_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateCreatedUtc)
                    .HasColumnName("date_created_utc")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(800)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(90);

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url")
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<ToolMedia>(entity =>
            {
                entity.ToTable("Tool_Media");

                entity.HasIndex(e => new { e.ToolId, e.MediaId })
                    .HasDatabaseName("UQ_dboTool_Media")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.ToolId).HasColumnName("tool_id");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.ToolMedia)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboTool_Media_dboMedia");

                entity.HasOne(d => d.Tool)
                    .WithMany(p => p.ToolMedia)
                    .HasForeignKey(d => d.ToolId)
                    .HasConstraintName("FK_dboTool_Media_dboTool");
            });

            modelBuilder.Entity<ToolSeries>(entity =>
            {
                entity.ToTable("Tool_Series");

                entity.HasIndex(e => new { e.ToolId, e.SeriesId })
                    .HasDatabaseName("UQ_dboTool_Series")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.SeriesId).HasColumnName("series_id");

                entity.Property(e => e.ToolId).HasColumnName("tool_id");

                entity.HasOne(d => d.Series)
                    .WithMany(p => p.ToolSeries)
                    .HasForeignKey(d => d.SeriesId)
                    .HasConstraintName("FK_dboTool_Series_dboSeries");

                entity.HasOne(d => d.Tool)
                    .WithMany(p => p.ToolSeries)
                    .HasForeignKey(d => d.ToolId)
                    .HasConstraintName("FK_dboTool_Series_dboTool");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboTopic_name")
                    .IsUnique();

                entity.HasIndex(e => e.ParentId)
                    .HasDatabaseName("IX_dboTopic_parent");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(400)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Logo)
                    .IsRequired()
                    .HasColumnName("logo")
                    .HasMaxLength(256)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.LogoMetadata).HasColumnName("logo_metadata");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(40);

                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_dboTopic_Self");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.PartnerId)
                    .HasDatabaseName("IX_dboUser_partner");

                entity.HasIndex(e => e.SocialLoginId)
                    .HasDatabaseName("UQF_dboUser_social_login")
                    .IsUnique()
                    .HasFilter("([social_login_id] IS NOT NULL)");

                entity.HasIndex(e => e.UserstatusId)
                    .HasDatabaseName("IX_dboUser_userstatus");

                entity.HasIndex(e => e.UsertypeId)
                    .HasDatabaseName("IX_dboUser_usertype");

                entity.HasIndex(e => new { e.LastName, e.FirstName, e.PartnerId, e.UsertypeId, e.UserstatusId, e.Password, e.Email })
                    .HasDatabaseName("UQF_dboUser_email")
                    .IsUnique()
                    .HasFilter("([email] IS NOT NULL)");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateCreatedUtc)
                    .HasColumnName("date_created_utc")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(100);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasMaxLength(25);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name")
                    .HasMaxLength(40);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(28)
                    .HasComputedColumnSql("(((ltrim(rtrim([first_name]))+space((1)))+left(ltrim(rtrim([last_name])),(1)))+'.')");

                entity.Property(e => e.PartnerId).HasColumnName("partner_id");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Phone)
                    .HasColumnName("phone")
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.SocialLoginId)
                    .HasColumnName("social_login_id")
                    .HasMaxLength(200);

                entity.Property(e => e.UserstatusId).HasColumnName("userstatus_id");

                entity.Property(e => e.UsertypeId).HasColumnName("usertype_id");

                entity.HasOne(d => d.Userstatus)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.UserstatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboUser_dboUserStatus");

                entity.HasOne(d => d.Usertype)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.UsertypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dboUser_dboUserType");
            });

            modelBuilder.Entity<UserStatus>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboUserStatus_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<UserType>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("UQ_dboUserType_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<WatchHistory>(entity =>
            {
                entity.HasIndex(e => new { e.LastWatchedUtc, e.UserId, e.MediaId })
                    .HasDatabaseName("UQ_dboWatchHistory")
                    .IsUnique();

                entity.HasIndex(e => new { e.UserId, e.LastWatchedUtc, e.MediaId })
                    .HasDatabaseName("IX_dboWatchHistory_media");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.LastWatchedUtc)
                    .HasColumnName("last_watched_utc")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.Rv)
                    .IsRequired()
                    .HasColumnName("rv")
                    .IsRowVersion();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.WatchHistory)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_dboWatchHistory_dboMedia");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.WatchHistory)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dboWatchHistory_dboUser");
            });
        }
    }
}
