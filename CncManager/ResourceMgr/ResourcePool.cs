using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CncManager
{
	// Resource snapshot with validity tracked separately from its value.
	public sealed class ResourceValue<T>
	{
		internal ResourceValue( int index, T value, bool isValid, long revision )
		{
			Index = index;
			Value = value;
			IsValid = isValid;
			Revision = revision;
		}

		// Resource address or index within its pool.
		public int Index
		{
			get;
		}

		// Last valid read value; retained when invalid, default(T) if never read.
		public T Value
		{
			get;
		}

		// False after a failed read or disconnection; check before using Value.
		public bool IsValid
		{
			get;
		}

		// Pool revision used to reject stale or duplicate notifications.
		internal long Revision
		{
			get;
		}
	}

	// Callbacks run on the calling/polling thread; UI subscribers must marshal to their UI thread.
	public abstract class ResourcePool<T> : IDisposable
	{
		#region Subscriptions

		public IDisposable Subscribe( int index, Action<ResourceValue<T>> callback )
		{
			if( index < 0 || callback == null ) {
				return null;
			}
			Subscription subscription;
			ResourceValue<T> current;
			lock( m_Lock ) {
				if( m_Disposed ) {
					return null;
				}
				Resource resource;
				if( !m_Resources.TryGetValue( index, out resource ) ) {
					resource = new Resource( index );
					m_Resources.Add( index, resource );
				}
				if( resource.Subscriptions.TryGetValue( callback, out subscription ) ) {
					return subscription;
				}
				subscription = new Subscription( this, resource, callback );
				resource.Subscriptions.Add( callback, subscription );
				current = resource.Current;
			}
			if( current != null ) {
				subscription.Notify( current );
			}
			return subscription;
		}

		public bool TryGetValue( int index, out ResourceValue<T> value )
		{
			lock( m_Lock ) {
				Resource resource;
				if( !m_Disposed && m_Resources.TryGetValue( index, out resource ) ) {
					value = resource.Current;
					return value != null;
				}
				value = null;
				return false;
			}
		}

		void Unsubscribe( Subscription subscription )
		{
			lock( m_Lock ) {
				subscription.Active = false;
				Resource resource = subscription.Resource;
				resource.Subscriptions.Remove( subscription.Callback );
				if( resource.Subscriptions.Count == 0 ) {
					m_Resources.Remove( resource.Index );
				}
			}
		}

		#endregion

		#region Connection and lifetime

		internal void SetConnected( bool connected, int connectionVersion )
		{
			List<Notification> notifications = new List<Notification>();
			lock( m_Lock ) {
				if( m_Disposed || connectionVersion < m_ConnectionVersion ) {
					return;
				}
				m_Connected = connected;
				m_ConnectionVersion = connectionVersion;
				if( !connected ) {
					foreach( Resource resource in m_Resources.Values ) {
						AddNotification( resource, false, default( T ), notifications );
					}
				}
			}
			Notify( notifications );
		}

		public void Dispose()
		{
			lock( m_Lock ) {
				if( m_Disposed ) {
					return;
				}
				m_Disposed = true;
				m_ConnectionVersion++;
				foreach( Resource resource in m_Resources.Values ) {
					foreach( Subscription subscription in resource.Subscriptions.Values ) {
						subscription.Active = false;
					}
				}
				m_Resources.Clear();
			}
		}

		#endregion

		#region Polling and notifications

		protected abstract Task<T> ReadOneResourceAsync( int index );

		internal async Task PollingAsync()
		{
			Resource[] resources;
			int connectionVersion;
			lock( m_Lock ) {
				if( m_Disposed || !m_Connected || m_Polling || m_Resources.Count == 0 ) {
					return;
				}
				m_Polling = true;
				connectionVersion = m_ConnectionVersion;
				resources = new Resource[ m_Resources.Count ];
				m_Resources.Values.CopyTo( resources, 0 );
			}
			try {
				foreach( Resource resource in resources ) {
					lock( m_Lock ) {
						if( m_Disposed || !m_Connected || connectionVersion != m_ConnectionVersion ) {
							break;
						}
						if( !IsRegisteredResource( resource ) ) {
							continue;
						}
					}
					T value = await ReadOneResourceAsync( resource.Index ).ConfigureAwait( false );
					List<Notification> notifications = new List<Notification>();
					lock( m_Lock ) {
						if( m_Disposed || !m_Connected || connectionVersion != m_ConnectionVersion || !IsRegisteredResource( resource ) ) {
							continue;
						}
						AddNotification( resource, (object)value != null, value, notifications );
					}
					Notify( notifications );
				}
			}
			finally {
				lock( m_Lock ) {
					m_Polling = false;
				}
			}
		}

		bool IsRegisteredResource( Resource resource )
		{
			Resource current;
			return m_Resources.TryGetValue( resource.Index, out current ) && current == resource;
		}

		void AddNotification( Resource resource, bool isValid, T value, List<Notification> notifications )
		{
			ResourceValue<T> previous = resource.Current;
			T lastValue = isValid ? value : previous == null ? default( T ) : previous.Value;
			if( previous != null && previous.IsValid == isValid && EqualityComparer<T>.Default.Equals( previous.Value, lastValue ) ) {
				return;
			}
			resource.Current = new ResourceValue<T>( resource.Index, lastValue, isValid, ++m_Revision );
			notifications.Add( new Notification( resource.Current, new List<Subscription>( resource.Subscriptions.Values ) ) );
		}

		void Notify( List<Notification> notifications )
		{
			foreach( Notification notification in notifications ) {
				foreach( Subscription subscription in notification.Subscriptions ) {
					subscription.Notify( notification.Value );
				}
			}
		}

		#endregion

		sealed class Resource
		{
			public Resource( int index )
			{
				Index = index;
			}

			public int Index
			{
				get;
			}

			public ResourceValue<T> Current;
			public readonly Dictionary<Action<ResourceValue<T>>, Subscription> Subscriptions = new Dictionary<Action<ResourceValue<T>>, Subscription>();
		}

		sealed class Notification
		{
			public Notification( ResourceValue<T> value, List<Subscription> subscriptions )
			{
				Value = value;
				Subscriptions = subscriptions;
			}

			public ResourceValue<T> Value
			{
				get;
			}

			public List<Subscription> Subscriptions
			{
				get;
			}
		}

		sealed class Subscription : IDisposable
		{
			public Subscription( ResourcePool<T> pool, Resource resource, Action<ResourceValue<T>> callback )
			{
				m_Pool = pool;
				Resource = resource;
				Callback = callback;
			}

			public Resource Resource
			{
				get;
			}

			public Action<ResourceValue<T>> Callback
			{
				get;
			}

			public bool Active = true;

			public void Notify( ResourceValue<T> value )
			{
				lock( m_Pool.m_Lock ) {
					if( !Active || m_Pool.m_Disposed || !m_Pool.IsRegisteredResource( Resource ) || Resource.Current != value || value.Revision <= m_LastRevision ) {
						return;
					}
					m_LastRevision = value.Revision;
				}
				try {
					Callback( value );
				}
				catch( Exception ex ) {
					System.Diagnostics.Trace.TraceError( "Resource {0} subscriber failed: {1}", Resource.Index, ex );
				}
			}

			public void Dispose()
			{
				lock( m_Pool.m_Lock ) {
					if( Active ) {
						m_Pool.Unsubscribe( this );
					}
				}
			}

			readonly ResourcePool<T> m_Pool;
			long m_LastRevision;
		}

		readonly object m_Lock = new object();
		readonly Dictionary<int, Resource> m_Resources = new Dictionary<int, Resource>();
		bool m_Connected;
		bool m_Polling;
		bool m_Disposed;
		int m_ConnectionVersion;
		long m_Revision;
	}
}
