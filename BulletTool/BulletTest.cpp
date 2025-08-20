#include <btBulletCollisionCommon.h>
#include <Gimpact/btGImpactShape.h>
#include <Gimpact/btGImpactCollisionAlgorithm.h>
#include <Dynamics/btDiscreteDynamicsWorld.h>

#include<map>

btCollisionShape *makeGImpactMeshShape( int triCount, int *indices, int vertCount, btScalar *verts )
{
	// stride: indices are ints, each triangle has 3 indices -> indexStride = 3*sizeof(int)
	btTriangleIndexVertexArray *meshInterface =
		new btTriangleIndexVertexArray( triCount, indices, 3 * sizeof( int ), vertCount, verts, 3 * sizeof( btScalar ) );

	// GImpact mesh shape supports moving concave meshes
	btGImpactMeshShape *gimpact = new btGImpactMeshShape( meshInterface );
	gimpact->updateBound();
	return gimpact;
}

struct BodyMeta
{
	std::string name; int index;
};

struct BodyRecord
{
	std::string name;
	btCollisionObject *obj;
	btCollisionShape *shape;
};

class CollisionScene
{
public:
	CollisionScene()
	{
		collisionConfiguration = new btDefaultCollisionConfiguration();
		dispatcher = new btCollisionDispatcher( collisionConfiguration );
		broadphase = new btDbvtBroadphase();
		// We only need collision detection, but use dynamics world if you later want dynamics
		dynamicsWorld = new btDiscreteDynamicsWorld( dispatcher, broadphase, nullptr, collisionConfiguration );
		// Register gimpact collision algorithm (required when using GImpact)
		btGImpactCollisionAlgorithm::registerAlgorithm( dispatcher );
	}

	~CollisionScene()
	{
		// cleanup: remove objects, delete shapes, world, etc. (omitted for brevity)
	}

	// add body (one-time). user provides name and collision shape (not yet transform)
	void addBody( const std::string &name, btCollisionShape *shape, int userIndex )
	{
		btCollisionObject *obj = new btCollisionObject();
		obj->setCollisionShape( shape );
		BodyMeta *meta = new BodyMeta(); meta->name = name; meta->index = userIndex;
		obj->setUserPointer( meta ); // store meta, remember to delete later
		dynamicsWorld->addCollisionObject( obj );
		BodyRecord r; r.name = name; r.obj = obj; r.shape = shape;
		bodies[ name ] = r;
	}

	// set transform of a body for current frame
	void setBodyTransform( const std::string &name, const btTransform &t )
	{
		auto it = bodies.find( name );
		if( it == bodies.end() ) return;
		it->second.obj->setWorldTransform( t );
	}

	// perform detection and check if bodyA/bodyB collides now
	bool checkCollisionPair( const std::string &bodyA, const std::string &bodyB )
	{
		dynamicsWorld->performDiscreteCollisionDetection();
		int numManifolds = dispatcher->getNumManifolds();
		for( int i = 0; i < numManifolds; ++i ) {
			btPersistentManifold *manifold = dispatcher->getManifoldByIndexInternal( i );
			if( manifold->getNumContacts() == 0 ) continue;
			const btCollisionObject *obA = static_cast< const btCollisionObject * >( manifold->getBody0() );
			const btCollisionObject *obB = static_cast< const btCollisionObject * >( manifold->getBody1() );
			BodyMeta *mA = static_cast< BodyMeta * >( obA->getUserPointer() );
			BodyMeta *mB = static_cast< BodyMeta * >( obB->getUserPointer() );
			if( !mA || !mB ) continue;
			// check name match (or index)
			if( ( mA->name == bodyA && mB->name == bodyB ) || ( mA->name == bodyB && mB->name == bodyA ) ) {
				return true;
			}
		}
		return false;
	}

	std::map<std::string, BodyRecord> bodies;

	// members
	btDefaultCollisionConfiguration *collisionConfiguration;
	btCollisionDispatcher *dispatcher;
	btBroadphaseInterface *broadphase;
	btDiscreteDynamicsWorld *dynamicsWorld;
};

