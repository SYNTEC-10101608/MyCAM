#pragma once
#include "FCLToolCommon.h"

#include <fcl/fcl.h>

class FCLTest {

public:
	void FCLTOOL_API AddModel( const std::string &szID,
		const int triCount, const int *const indexList,
		const int vertexCount, const double *const vertexList );

	bool FCLTOOL_API  CheckCollision( const std::string &szID1, const std::string &szID2,
		const double *const trsf1, const double *const trsf2 );

private:
	fcl::Transform3f CreateTransform( const double *const trsf );
	std::unordered_map<std::string, std::shared_ptr<fcl::BVHModel<fcl::OBBRSSf>>> m_ModelMap;
};
