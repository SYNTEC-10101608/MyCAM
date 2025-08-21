#pragma once
#include "FCLToolCommon.h"

#include <fcl/fcl.h>

class FCLTest
{

public:
	void FCLTOOL_API AddModel( const std::string &szID,
		const int triCount, const int *indexList,
		const int vertexCount, const double *vertexList );

	bool FCLTOOL_API  CheckCollision( const std::string &szID1, const std::string &szID2,
		const double *trsf1, const double *trsf2 );

private:
	fcl::Transform3f CreateTransform( const double *trsf );
	std::unordered_map<std::string, std::shared_ptr<fcl::BVHModel<fcl::OBBRSSf>>> m_ModelMap;
};
