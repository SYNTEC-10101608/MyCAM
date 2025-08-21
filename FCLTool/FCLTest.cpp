#include "FCLTest.h"

#pragma warning(disable:4146)

using namespace fcl;

void FCLTest::AddModel( const std::string &szID,
	const int triCount, const int *indexList,
	const int vertexCount, const double *vertexList )
{
	std::vector<Vector3f> vertices;
	std::vector<Triangle> triangles;
	for( int i = 0; i < triCount; i++ ) {
		int index = indexList[ i ];
		Triangle tri;
		tri.set( indexList[ i * 3 + 0 ], indexList[ i * 3 + 1 ], indexList[ i * 3 + 2 ] );
		triangles.push_back( tri );
	}
	for( int i = 0; i < vertexCount; i++ ) {
		Vector3f v( vertexList[ i * 3 + 0 ], vertexList[ i * 3 + 1 ], vertexList[ i * 3 + 2 ] );
		vertices.push_back( v );
	}
	typedef BVHModel<OBBRSSf> Model;
	std::shared_ptr<Model> geom = std::make_shared<Model>();
	geom->beginModel();
	geom->addSubModel( vertices, triangles );
	geom->endModel();
	m_ModelMap[ szID ] = geom;
}

bool FCLTest::CheckCollision( const std::string &szID1, const std::string &szID2,
	const double *trsf1, const double *trsf2 )
{
	auto it1 = m_ModelMap.find( szID1 );
	auto it2 = m_ModelMap.find( szID2 );
	if( it1 == m_ModelMap.end() || it2 == m_ModelMap.end() ) {
		return false;
	}

	// Create transforms from the input arrays
	Transform3f pose1 = CreateTransform( trsf1 );
	Transform3f pose2 = CreateTransform( trsf2 );

	// create object
	const auto &model1 = it1->second;
	const auto &model2 = it2->second;
	CollisionObjectf obj1( model1, pose1 );
	CollisionObjectf obj2( model2, pose2 );
	CollisionRequestf request;
	CollisionResultf result;
	collide( &obj1, &obj2, request, result );
	return result.isCollision();
}

Transform3f FCLTest::CreateTransform( const double *trsf )
{
	Matrix3f R;
	Vector3f T;
	R << trsf[ 0 ], trsf[ 1 ], trsf[ 2 ],
		trsf[ 3 ], trsf[ 4 ], trsf[ 5 ],
		trsf[ 6 ], trsf[ 7 ], trsf[ 8 ];
	T << trsf[ 9 ], trsf[ 10 ], trsf[ 11 ];
	Transform3f pose = Transform3f::Identity();
	pose.linear() = R;
	pose.translation() = T;
	return pose;
}
