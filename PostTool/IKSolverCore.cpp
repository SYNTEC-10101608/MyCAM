#include "IKSolverCore.h"

using namespace Eigen;

#define MIN_EPSILON_THRESHOLD		( DBL_EPSILON * 1.0e6 )	// threshold for calculate in double type
#define EPSILON_double				( DBL_EPSILON * 1.0e6 )	// threshold for calculate in double type

namespace PostTool
{
	int m_IUtoBLU_Rotary = 1;
	int ALMID_CrdNoSolutionForThisDirection = 1;
	int ALMID_NoError = 0;

	bool SolveQuadEq( double a, double b, double c, double &x1, double &x2 )
	{
		constexpr double EPSILON = std::numeric_limits<double>::epsilon();

		if( std::fabs( a ) < EPSILON ) {
			if( std::fabs( b ) < EPSILON ) {
				return false; // 無解或無限多解
			}
			x1 = x2 = -c / b;
			return true;
		}

		double discriminant = b * b - 4 * a * c;
		if( discriminant < -EPSILON ) {
			return false; // 無實數解
		}

		if( std::fabs( discriminant ) < EPSILON ) {
			x1 = x2 = -b / ( 2 * a );
			return true;
		}

		double sqrt_d = std::sqrt( discriminant );
		x1 = ( -b + sqrt_d ) / ( 2 * a );
		x2 = ( -b - sqrt_d ) / ( 2 * a );
		return true;
	}

	double Determinant( double a, double b, double c, double d )
	{
		return a * d - b * c;
	}

	int IKSolverCore::IJKtoMS(
		Vector3d ToolDirection,
		const Vector3d &ToolDirectionAtZero,
		const Vector3d &DirectOfFirstRotAxis,
		const Vector3d &DirectOfSecondRotAxis,
		double LastMasterRotAngle,
		double LastSlaveRotAngle,
		double &MRotAngle1,
		double &SRotAngle1,
		double &MRotAngle2,
		double &SRotAngle2 )
	{
		double MSin1, MSin2, MCos1, MCos2, SSin1, SSin2, SCos1, SCos2;
		Vector3d M1, M2, S1, S2;
		double A, B, C;
		double SM, SM1, SM2, MK, M1K, M2K, SD, S1D, S2D, S1M, S1M1, S1M2, S2M, S2M1, S2M2;
		double coeff1, coeff2, coeff3;
		bool bSolvable;

		ToolDirection.normalize();
		SM = DirectOfSecondRotAxis.dot( DirectOfFirstRotAxis );
		SD = DirectOfSecondRotAxis.dot( ToolDirectionAtZero );
		MK = DirectOfFirstRotAxis.dot( ToolDirection );

		M1 = ToolDirection.cross( DirectOfFirstRotAxis );

		if( M1.norm() >= MIN_EPSILON_THRESHOLD ) {
			M1.normalize();
			M2 = DirectOfFirstRotAxis.cross( M1 );
			M2.normalize();

			SM1 = DirectOfSecondRotAxis.dot( M1 );
			SM2 = DirectOfSecondRotAxis.dot( M2 );
			M1K = M1.dot( ToolDirection );
			M2K = M2.dot( ToolDirection );
			coeff1 = SM1 * M1K + SM2 * M2K;
			coeff2 = -SM2 * M1K + SM1 * M2K;
			coeff3 = SD - SM * MK;

			A = pow( coeff1, 2 ) + pow( coeff2, 2 );
			assert( fabs( A ) >= EPSILON_double );

			if( fabs( coeff1 ) >= fabs( coeff2 ) ) {
				B = -2 * coeff2 * coeff3;
				C = pow( coeff3, 2 ) - pow( coeff1, 2 );
				bSolvable = SolveQuadEq( A, B, C, MSin1, MSin2 );

				MCos1 = ( coeff3 - coeff2 * MSin1 ) / coeff1;
				MCos2 = ( coeff3 - coeff2 * MSin2 ) / coeff1;
			}
			else {
				B = -2 * coeff1 * coeff3;
				C = pow( coeff3, 2 ) - pow( coeff2, 2 );
				bSolvable = SolveQuadEq( A, B, C, MCos1, MCos2 );

				MSin1 = ( coeff3 - coeff1 * MCos1 ) / coeff2;
				MSin2 = ( coeff3 - coeff1 * MCos2 ) / coeff2;
			}

			if( bSolvable ) {
				MRotAngle1 = atan2( MSin1, MCos1 ) * m_IUtoBLU_Rotary;
				MRotAngle2 = atan2( MSin2, MCos2 ) * m_IUtoBLU_Rotary;

				S1 = ToolDirectionAtZero.cross( DirectOfSecondRotAxis );
				if( S1.norm() >= MIN_EPSILON_THRESHOLD ) {
					S1.normalize();
					S2 = DirectOfSecondRotAxis.cross( S1 );
					S2.normalize();

					S1D = S1.dot( ToolDirectionAtZero );
					S2D = S2.dot( ToolDirectionAtZero );
					S1M = S1.dot( DirectOfFirstRotAxis );
					S1M1 = S1.dot( M1 );
					S1M2 = S1.dot( M2 );
					S2M = S2.dot( DirectOfFirstRotAxis );
					S2M1 = S2.dot( M1 );
					S2M2 = S2.dot( M2 );

					double temp1 = S1M * MK + ( S1M1 * M1K + S1M2 * M2K ) * MCos1 + ( -S1M2 * M1K + S1M1 * M2K ) * MSin1;
					double temp2 = S2M * MK + ( S2M1 * M1K + S2M2 * M2K ) * MCos1 + ( -S2M2 * M1K + S2M1 * M2K ) * MSin1;
					double temp3 = S1M * MK + ( S1M1 * M1K + S1M2 * M2K ) * MCos2 + ( -S1M2 * M1K + S1M1 * M2K ) * MSin2;
					double temp4 = S2M * MK + ( S2M1 * M1K + S2M2 * M2K ) * MCos2 + ( -S2M2 * M1K + S2M1 * M2K ) * MSin2;

					SCos1 = Determinant( temp1, -S2D, temp2, S1D );
					SSin1 = Determinant( S1D, temp1, S2D, temp2 );
					SCos2 = Determinant( temp3, -S2D, temp4, S1D );
					SSin2 = Determinant( S1D, temp3, S2D, temp4 );

					SRotAngle1 = atan2( SSin1, SCos1 ) * m_IUtoBLU_Rotary;
					SRotAngle2 = atan2( SSin2, SCos2 ) * m_IUtoBLU_Rotary;
				}
				else {
					SRotAngle1 = LastSlaveRotAngle;
					SRotAngle2 = LastSlaveRotAngle;
				}
			}
			else {
				return ALMID_CrdNoSolutionForThisDirection;
			}
		}
		else {
			if( fabs( SD - SM * MK ) >= MIN_EPSILON_THRESHOLD ) {
				return ALMID_CrdNoSolutionForThisDirection;
			}
			else {
				S1 = ToolDirectionAtZero.cross( DirectOfSecondRotAxis );
				S1.normalize();
				S2 = DirectOfSecondRotAxis.cross( S1 );
				S2.normalize();

				MRotAngle1 = LastMasterRotAngle;
				MRotAngle2 = LastMasterRotAngle;

				S1D = S1.dot( ToolDirectionAtZero );
				S2D = S2.dot( ToolDirectionAtZero );
				S1M = S1.dot( DirectOfFirstRotAxis );
				S2M = S2.dot( DirectOfFirstRotAxis );

				SSin1 = -S2D * S1M * MK + S1D * S2M * MK;
				SCos1 = S1D * S1M * MK + S2D * S2M * MK;

				SRotAngle1 = atan2( SSin1, SCos1 ) * m_IUtoBLU_Rotary;
				SRotAngle2 = SRotAngle1;
			}
		}

		return ALMID_NoError;
	}
}
