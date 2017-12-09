﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Nightingale.Metadata;
using Nightingale.Sessions;

namespace Nightingale.Entities
{
    public abstract class Entity : INotifyPropertyChanged
    {
        /// <summary>
        /// Raises when a property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _deleted;
		private int _version;

        /// <summary>
        /// Gets the id.
        /// </summary>
        [Mapped]
        [PrimaryKey]
        [Cascade(Cascade.None)]
        [FieldType("int")]
        [Description("Gets the id.")]
        public int Id { get; internal set; }

        /// <summary>
        /// A value indicating whether the entity is marked as deleted.
        /// </summary>
        [Mapped]
        [Cascade(Cascade.None)]
        [FieldType("bool")]
        [Description("A value indicating whether the entity is marked as deleted.")]
        public bool Deleted
        {
            get => _deleted;
            internal set
            {
                PropertyChangeTracker.AddPropertyChangedItem(nameof(Deleted), _deleted, value);
                _deleted = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A value indicating the version of the entity.
        /// </summary>
        [Mapped]
        [Cascade(Cascade.None)]
        [FieldType("int")]
        [Description("A value indicating the version of the entity.")]
		public int Version 
		{ 
			get => _version;
            internal set
			{
                PropertyChangeTracker.AddPropertyChangedItem(nameof(Version), _version, value);
				_version = value;

			    OnPropertyChanged();
            }
		}

        /// <summary>
        /// A value indicating whether the entity is saved.
        /// </summary>
        public bool IsSaved => Id != 0;

        /// <summary>
        /// A value indicating whether the entity is not saved.
        /// </summary>
        public bool IsNotSaved => !IsSaved;

        /// <summary>
        /// Gets the property change tracker.
        /// </summary>
        public PropertyChangeTracker PropertyChangeTracker { get; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        protected Session Session { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Nightingale.Entities.Entity"/> is Evicted.
		/// </summary>
		internal bool Evicted { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nightingale.Entities.Entity"/> class.
        /// </summary>
        protected Entity()
		{
            PropertyChangeTracker = new PropertyChangeTracker(this);
		}

        /// <summary>
        /// Sets the session.
        /// </summary>
        /// <param name="session">The session.</param>
        internal void SetSession(Session session)
        {
            Session = session;
        }

        /// <summary>
        /// Loads all properties wich are marked with eager loading.
        /// </summary>
        internal void EagerLoadPropertiesInternal()
        {
            EagerLoadProperties();
        }

        /// <summary>
        /// Loads all properties wich are marked with eager loading.
        /// </summary>
        protected abstract void EagerLoadProperties();

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <returns>Returns true if the entity is valid and ready to save.</returns>
        public virtual bool Validate()
        {
            var properties = ReflectionHelper.GetProperties(GetType());
            var metadata = DependencyResolver.GetInstance<IEntityMetadataResolver>().GetEntityMetadata(this);

            return
                metadata.Fields.Where(x => x.Mandatory)
                    .Select(fieldMetadata => properties.First(x => x.Name == fieldMetadata.Name))
                    .All(property => property.GetValue(this) != null);
        }

        /// <summary>
        /// Should be called when a property was changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
