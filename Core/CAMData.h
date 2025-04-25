#pragma once

#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <tuple>
#include <memory>
#include <gp_Pnt.hxx>
#include <gp_Dir.hxx>
#include <gp_Vec.hxx>

#include "CADData.h"

using namespace Core::DataStructure;

namespace Core
{
	namespace DataStructure
	{
		enum class ToolVectorType
		{
			Default,
			Intersecting,
			TowardZ
		};

		class CADPoint
		{
		public:
			CADPoint( const gp_Pnt &point, const gp_Dir &normalVec, const gp_Dir &tangentVec );

			const gp_Pnt &GetPoint() const;
			const gp_Dir &GetNormalVec() const;
			const gp_Dir &GetTangentVec() const;

		private:
			gp_Pnt m_Point;
			gp_Dir m_NormalVec;
			gp_Dir m_TangentVec;
		};

		class CAMPoint
		{
		public:
			CAMPoint( const std::shared_ptr<CADPoint> &cadPoint, const gp_Dir &toolVec );

			const std::shared_ptr<CADPoint> &GetCADPoint() const;
			const gp_Dir &GetToolVec() const;

		private:
			std::shared_ptr<CADPoint> m_CADPoint;
			gp_Dir m_ToolVec;
		};

		class CAMData
		{
		public:
			CAMData( const std::shared_ptr<CADData> &cadData );

			const std::vector<std::shared_ptr<CADPoint>> &GetCADPointList() const;
			const std::vector<std::shared_ptr<CAMPoint>> &GetCAMPointList();

			bool IsReverse() const;
			void SetReverse( bool isReverse );

			int GetStartPoint() const;
			void SetStartPoint( int startPoint );

			double GetOffset() const;
			void SetOffset( double offset );

			void SetToolVecModify( int index, double dRA_deg, double dRB_deg );
			void GetToolVecModify( int index, double &dRA_deg, double &dRB_deg ) const;
			std::unordered_set<int> GetToolVecModifyIndex() const;

		private:
			void BuildCADPointList();
			void BuildCAMPointList();
			void SetToolVec();
			void ModifyToolVec();
			void SetStartPointInternal();
			void SetOrientation();

			std::shared_ptr<CADData> m_CADData;
			std::vector<std::shared_ptr<CADPoint>> m_CADPointList;
			std::vector<std::shared_ptr<CAMPoint>> m_CAMPointList;
			std::unordered_map<int, std::tuple<double, double>> m_ToolVecModifyMap;
			bool m_IsReverse = false;
			int m_StartPoint = 0;
			double m_Offset = 0.0;
			bool m_IsDirty = false;
		};
	}
}
