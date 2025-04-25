#include "CAMData.h"  
#include <algorithm>  

using namespace Core::DataStructure;

CADPoint::CADPoint( const gp_Pnt &point, const gp_Dir &normalVec, const gp_Dir &tangentVec )
	: m_Point( point ), m_NormalVec( normalVec ), m_TangentVec( tangentVec )
{
}

const gp_Pnt &CADPoint::GetPoint() const
{
	return m_Point;
}
const gp_Dir &CADPoint::GetNormalVec() const
{
	return m_NormalVec;
}
const gp_Dir &CADPoint::GetTangentVec() const
{
	return m_TangentVec;
}

CAMPoint::CAMPoint( const std::shared_ptr<CADPoint> &cadPoint, const gp_Dir &toolVec )
	: m_CADPoint( cadPoint ), m_ToolVec( toolVec )
{
}

const std::shared_ptr<CADPoint> &CAMPoint::GetCADPoint() const
{
	return m_CADPoint;
}
const gp_Dir &CAMPoint::GetToolVec() const
{
	return m_ToolVec;
}

CAMData::CAMData( const std::shared_ptr<CADData> &cadData )
	: m_CADData( cadData )
{
	BuildCADPointList();
	BuildCAMPointList();
}

const std::vector<std::shared_ptr<CADPoint>> &CAMData::GetCADPointList() const
{
	return m_CADPointList;
}

const std::vector<std::shared_ptr<CAMPoint>> &CAMData::GetCAMPointList()
{
	if( m_IsDirty ) {
		BuildCAMPointList();
		m_IsDirty = false;
	}
	return m_CAMPointList;
}

bool CAMData::IsReverse() const
{
	return m_IsReverse;
}
void CAMData::SetReverse( bool isReverse )
{
	if( m_IsReverse != isReverse ) {
		m_IsReverse = isReverse;
		m_IsDirty = true;
	}
}

int CAMData::GetStartPoint() const
{
	return m_StartPoint;
}
void CAMData::SetStartPoint( int startPoint )
{
	if( m_StartPoint != startPoint ) {
		m_StartPoint = startPoint;
		m_IsDirty = true;
	}
}

double CAMData::GetOffset() const
{
	return m_Offset;
}
void CAMData::SetOffset( double offset )
{
	if( m_Offset != offset ) {
		m_Offset = offset;
		m_IsDirty = true;
	}
}

void CAMData::SetToolVecModify( int index, double dRA_deg, double dRB_deg )
{
	m_ToolVecModifyMap[ index ] = std::make_tuple( dRA_deg, dRB_deg );
	m_IsDirty = true;
}

void CAMData::GetToolVecModify( int index, double &dRA_deg, double &dRB_deg ) const
{
	auto it = m_ToolVecModifyMap.find( index );
	if( it != m_ToolVecModifyMap.end() ) {
		dRA_deg = std::get<0>( it->second );
		dRB_deg = std::get<1>( it->second );
	}
	else {
		dRA_deg = 0.0;
		dRB_deg = 0.0;
	}
}

std::unordered_set<int> CAMData::GetToolVecModifyIndex() const
{
	std::unordered_set<int> result;
	for( const auto &pair : m_ToolVecModifyMap ) {
		result.insert( pair.first );
	}
	return result;
}

void CAMData::BuildCADPointList()
{
	// Implementation of CADPointList construction  
}

void CAMData::BuildCAMPointList()
{
	m_IsDirty = false;
	m_CAMPointList.clear();
	SetToolVec();
	SetStartPointInternal();
	SetOrientation();
}

void CAMData::SetToolVec()
{
	for( const auto &cadPoint : m_CADPointList ) {
		gp_Dir toolVec = cadPoint->GetNormalVec().Crossed( cadPoint->GetTangentVec() );
		m_CAMPointList.emplace_back( std::make_shared<CAMPoint>( cadPoint, toolVec ) );
	}
	ModifyToolVec();
}

void CAMData::ModifyToolVec()
{
	if( m_ToolVecModifyMap.empty() ) return;

	std::vector<int> indices;
	for( const auto &pair : m_ToolVecModifyMap ) {
		indices.push_back( pair.first );
	}
	std::sort( indices.begin(), indices.end() );

	// Modify tool vectors logic  
}

void CAMData::SetStartPointInternal()
{
	if( m_StartPoint != 0 ) {
		std::rotate( m_CAMPointList.begin(),
			m_CAMPointList.begin() + m_StartPoint,
			m_CAMPointList.end() );
	}
}

void CAMData::SetOrientation()
{
	if( m_IsReverse ) {
		std::reverse( m_CAMPointList.begin(), m_CAMPointList.end() );
	}
}