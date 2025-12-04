#include "FiveAxisSolverCore.h"
#include <cmath>
#include <limits>

using namespace Eigen;

// Define PI constant if not available
#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#define MIN_EPSILON_THRESHOLD		( DBL_EPSILON * 1.0e6 )
#define EPSILON_double				( DBL_EPSILON * 1.0e6 )
#define FIVEAXIS_SOL_TOL			( 0.001 )
#define REACHABLE_EPSILON			( 1.0e-6 )

namespace PostTool
{
	// Error codes
	const int ALMID_NoError = 0;
	const int ALMID_NoSolution = 1;
	const int ALMID_MasterInfinityOfSolution = 2;
	const int ALMID_SlaveInfinityOfSolution = 3;

	bool FiveAxisSolverCore::SolveQuadEq( double a, double b, double c, double& x1, double& x2 )
	{
		constexpr double EPSILON = std::numeric_limits<double>::epsilon() * 1.0e6;

		if( std::fabs( a ) < EPSILON ) {
			if( std::fabs( b ) < EPSILON ) {
				return false;
			}
			x1 = x2 = -c / b;
			return true;
		}

		double discriminant = b * b - 4 * a * c;
		if( discriminant < -EPSILON ) {
			return false;
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

	double FiveAxisSolverCore::Determinant( double a, double b, double c, double d )
	{
		return a * d - b * c;
	}

	void FiveAxisSolverCore::ConvertAngleRange( double& angle, double IUtoBLU_Rotary )
	{
		const double PI = M_PI * IUtoBLU_Rotary;
		const double DoublePI = 2 * PI;

		// Convert to 0 ~ 2PI
		angle = angle - std::floor( angle / DoublePI ) * DoublePI;

		// Convert to -PI ~ PI
		if( angle >= PI ) {
			angle -= DoublePI;
		}

		if( angle <= -PI ) {
			angle += DoublePI;
		}
	}

	bool FiveAxisSolverCore::IsAtPermissibleRange( double Angle, double start, double end, double IUtoBLU_Rotary )
	{
		ConvertAngleRange( Angle, IUtoBLU_Rotary );

		if( end == start ) {
			return true;
		}

		if( ( Angle < start ) && ( ( start - Angle ) < MIN_EPSILON_THRESHOLD ) ) {
			return true;
		}

		if( ( ( Angle > end ) && ( ( Angle - end ) < MIN_EPSILON_THRESHOLD ) ) ||
			( ( end == M_PI * IUtoBLU_Rotary ) && ( Angle < ( -end + MIN_EPSILON_THRESHOLD ) ) ) ) {
			return true;
		}

		if( end > start ) {
			if( ( Angle >= start ) && ( Angle <= end ) ) {
				return true;
			}
		}
		else {
			if( !( ( Angle > end ) && ( Angle < start ) ) ) {
				return true;
			}
		}

		return false;
	}

	bool FiveAxisSolverCore::IsPathPermissible( double Tar, double LastPos, double Start, double End, double IUtoBLU_Rotary )
	{
		if( Start == End ) {
			return true;
		}

		double CachedLastPos = LastPos;
		ConvertAngleRange( LastPos, IUtoBLU_Rotary );
		Tar += ( LastPos - CachedLastPos );

		End += MIN_EPSILON_THRESHOLD;
		Start -= MIN_EPSILON_THRESHOLD;

		const double PI = M_PI * IUtoBLU_Rotary;

		if( End > Start ) {
			if( ( End >= LastPos ) && ( LastPos >= Start ) ) {
				return ( End >= Tar ) && ( Tar >= Start );
			}
			else {
				return false;
			}
		}
		else {
			if( End > LastPos ) {
				return ( End >= Tar ) && ( Tar >= Start - 2 * PI );
			}
			else if( LastPos > Start ) {
				return ( End + 2 * PI >= Tar ) && ( Tar >= Start );
			}
			else {
				return false;
			}
		}
	}

	bool FiveAxisSolverCore::ToPermissibleCoterminalAng( double& TarPos, double LastPos, SolutionType Prefer,
		double LimitStart, double LimitEnd, int nRDofAX, double IUtoBLU_Rotary )
	{
		const double PI = M_PI;
		const double BLUtoIU_Rotary = 1.0 / IUtoBLU_Rotary;

		int nRev = static_cast<int>( std::floor( ( LastPos - TarPos ) / PI * BLUtoIU_Rotary * 0.5 ) );
		double CoterAng[2];

		CoterAng[0] = TarPos + 2 * ( nRev + 1 ) * PI * IUtoBLU_Rotary;
		if( std::fabs( LastPos - CoterAng[0] ) < FIVEAXIS_SOL_TOL ) {
			TarPos = CoterAng[0];
			return true;
		}

		CoterAng[1] = TarPos + 2 * nRev * PI * IUtoBLU_Rotary;
		if( std::fabs( LastPos - CoterAng[1] ) < FIVEAXIS_SOL_TOL ) {
			TarPos = CoterAng[1];
			return true;
		}

		int nSolution = 0;
		bool bPermissible[2];
		bPermissible[0] = IsPathPermissible( CoterAng[0], LastPos, LimitStart, LimitEnd, IUtoBLU_Rotary );
		bPermissible[1] = IsPathPermissible( CoterAng[1], LastPos, LimitStart, LimitEnd, IUtoBLU_Rotary );

		switch( Prefer ) {
		case SolutionType::MasterPos:
			nSolution = ( nRDofAX > 0 ) ? 0 : 1;
			break;

		case SolutionType::MasterNeg:
			nSolution = ( nRDofAX > 0 ) ? 1 : 0;
			break;

		case SolutionType::ShortestDist:
		case SolutionType::MSAngleShortestDist:
			if( bPermissible[0] == true ) {
				if( bPermissible[1] == true ) {
					double Tar0Dist = CoterAng[0] - LastPos;
					double Tar1Dist = LastPos - CoterAng[1];

					if( std::fabs( Tar0Dist - Tar1Dist ) > REACHABLE_EPSILON ) {
						nSolution = ( Tar0Dist < Tar1Dist ) ? 0 : 1;
					}
					else {
						nSolution = ( nRDofAX > 0 ) ? 0 : 1;
					}
				}
				else {
					nSolution = 0;
				}
			}
			else if( bPermissible[1] == true ) {
				nSolution = 1;
			}
			else {
				return false;
			}
			break;

		default:
			return false;
		}

		if( bPermissible[nSolution] == true ) {
			TarPos = CoterAng[nSolution];
		}
		return bPermissible[nSolution];
	}

	bool FiveAxisSolverCore::IsSolutionPermissible( double& MasterTarPos, double& SlaveTarPos,
		double MasterLastPos, double SlaveLastPos, SolutionType type,
		double FStart, double FEnd, double SStart, double SEnd,
		int nRDOfFirst, int nRDOfSecond, double IUtoBLU_Rotary )
	{
		bool bTarPosPermissible = IsAtPermissibleRange( MasterTarPos, FStart, FEnd, IUtoBLU_Rotary ) &&
			IsAtPermissibleRange( SlaveTarPos, SStart, SEnd, IUtoBLU_Rotary );

		if( bTarPosPermissible == true ) {
			bool bPathPermissible = ToPermissibleCoterminalAng( MasterTarPos, MasterLastPos, type, FStart, FEnd, nRDOfFirst, IUtoBLU_Rotary ) &&
				ToPermissibleCoterminalAng( SlaveTarPos, SlaveLastPos, SolutionType::ShortestDist, SStart, SEnd, nRDOfSecond, IUtoBLU_Rotary );
			return bPathPermissible;
		}

		return false;
	}

	int FiveAxisSolverCore::IJKtoMS(
		Vector3d ToolDirection,
		const Vector3d& ToolDirectionAtZero,
		const Vector3d& DirectOfFirstRotAxis,
		const Vector3d& DirectOfSecondRotAxis,
		double LastMasterRotAngle,
		double LastSlaveRotAngle,
		double& MRotAngle1,
		double& SRotAngle1,
		double& MRotAngle2,
		double& SRotAngle2,
		double IUtoBLU_Rotary )
	{
		double MSin1, MSin2, MCos1, MCos2, SSin1, SSin2, SCos1, SCos2;
		Vector3d M1, M2, S1, S2;
		double A, B, C;
		double SM, SM1, SM2, MK, M1K, M2K, SD, S1D, S2D, S1M, S1M1, S1M2, S2M, S2M1, S2M2;
		double coeff1, coeff2, coeff3;
		bool bSolvable;

		// M: DirectOfFirstRotAxis, S: DirectOfSecondRotAxis, D: ToolDirectionAtZero, K: ToolDirection
		ToolDirection.normalize();
		SM = DirectOfSecondRotAxis.dot( DirectOfFirstRotAxis );
		SD = DirectOfSecondRotAxis.dot( ToolDirectionAtZero );
		MK = DirectOfFirstRotAxis.dot( ToolDirection );

		M1 = ToolDirection.cross( DirectOfFirstRotAxis ); // M'

		// Compute the solution of rotation angle
		if( M1.norm() >= MIN_EPSILON_THRESHOLD ) {
			M1.normalize();
			M2 = DirectOfFirstRotAxis.cross( M1 ); // M"
			M2.normalize();

			SM1 = DirectOfSecondRotAxis.dot( M1 );	// SM1 = S x M'
			SM2 = DirectOfSecondRotAxis.dot( M2 );	// SM2 = S x M"
			M1K = M1.dot( ToolDirection );			// M1K = M' x K
			M2K = M2.dot( ToolDirection );			// M2K = M" x K
			coeff1 = SM1 * M1K + SM2 * M2K;
			coeff2 = -SM2 * M1K + SM1 * M2K;
			coeff3 = SD - SM * MK;

			A = std::pow( coeff1, 2 ) + std::pow( coeff2, 2 );

			// Compute MRotAngle
			if( std::fabs( coeff1 ) >= std::fabs( coeff2 ) ) {
				// coefficient of the quadratic equation of sin
				B = -2 * coeff2 * coeff3;
				C = std::pow( coeff3, 2 ) - std::pow( coeff1, 2 );
				bSolvable = SolveQuadEq( A, B, C, MSin1, MSin2 );

				// solve this equation
				MCos1 = ( coeff3 - coeff2 * MSin1 ) / coeff1;
				MCos2 = ( coeff3 - coeff2 * MSin2 ) / coeff1;
			}
			else {
				// coefficient of quadratic equation of cos
				B = -2 * coeff1 * coeff3;
				C = std::pow( coeff3, 2 ) - std::pow( coeff2, 2 );
				bSolvable = SolveQuadEq( A, B, C, MCos1, MCos2 );

				// solve this equation
				MSin1 = ( coeff3 - coeff1 * MCos1 ) / coeff2;
				MSin2 = ( coeff3 - coeff1 * MCos2 ) / coeff2;
			}

			// the equation of sin or cos has solution
			if( bSolvable ) {
				// use atan2 to compute the master angle
				MRotAngle1 = std::atan2( MSin1, MCos1 ) * IUtoBLU_Rotary;
				MRotAngle2 = std::atan2( MSin2, MCos2 ) * IUtoBLU_Rotary;

				// compute SRotAngle
				S1 = ToolDirectionAtZero.cross( DirectOfSecondRotAxis ); // S'
				if( S1.norm() >= MIN_EPSILON_THRESHOLD ) {
					S1.normalize();
					S2 = DirectOfSecondRotAxis.cross( S1 ); // S"
					S2.normalize();

					S1D = S1.dot( ToolDirectionAtZero );	// S1D = S' x D
					S2D = S2.dot( ToolDirectionAtZero );	// S2D = S" x D
					S1M = S1.dot( DirectOfFirstRotAxis );	// S1M = S' x M
					S1M1 = S1.dot( M1 );					// S1M1 = S' x M'
					S1M2 = S1.dot( M2 );					// S1M2 = S' x M"
					S2M = S2.dot( DirectOfFirstRotAxis );	// S2M = S" x M
					S2M1 = S2.dot( M1 );					// S2M1 = S" x M'
					S2M2 = S2.dot( M2 );					// S2M2 = S" x M"

					double temp1 = S1M * MK + ( S1M1 * M1K + S1M2 * M2K ) * MCos1 + ( -S1M2 * M1K + S1M1 * M2K ) * MSin1;
					double temp2 = S2M * MK + ( S2M1 * M1K + S2M2 * M2K ) * MCos1 + ( -S2M2 * M1K + S2M1 * M2K ) * MSin1;
					double temp3 = S1M * MK + ( S1M1 * M1K + S1M2 * M2K ) * MCos2 + ( -S1M2 * M1K + S1M1 * M2K ) * MSin2;
					double temp4 = S2M * MK + ( S2M1 * M1K + S2M2 * M2K ) * MCos2 + ( -S2M2 * M1K + S2M1 * M2K ) * MSin2;

					SCos1 = Determinant( temp1, -S2D, temp2, S1D );
					SSin1 = Determinant( S1D, temp1, S2D, temp2 );
					SCos2 = Determinant( temp3, -S2D, temp4, S1D );
					SSin2 = Determinant( S1D, temp3, S2D, temp4 );

					// use atan2 to compute the slave angle
					SRotAngle1 = std::atan2( SSin1, SCos1 ) * IUtoBLU_Rotary;
					SRotAngle2 = std::atan2( SSin2, SCos2 ) * IUtoBLU_Rotary;
					return ALMID_NoError;
				}
				else {
					// slave has infinity of solution, use last slave angle
					SRotAngle1 = LastSlaveRotAngle;
					SRotAngle2 = LastSlaveRotAngle;
					return ALMID_SlaveInfinityOfSolution;
				}
			}
			else {
				return ALMID_NoSolution;
			}
		}
		else {
			if( std::fabs( SD - SM * MK ) >= MIN_EPSILON_THRESHOLD ) {
				return ALMID_NoSolution;
			}
			else {
				S1 = ToolDirectionAtZero.cross( DirectOfSecondRotAxis ); // S'
				S1.normalize();
				S2 = DirectOfSecondRotAxis.cross( S1 ); // S"
				S2.normalize();

				// master has infinity of solution, use last master angle
				MRotAngle1 = LastMasterRotAngle;
				MRotAngle2 = LastMasterRotAngle;

				S1D = S1.dot( ToolDirectionAtZero ); // S1D = S' x D
				S2D = S2.dot( ToolDirectionAtZero ); // S2D = S" x D
				S1M = S1.dot( DirectOfFirstRotAxis ); // S1M = S' x M
				S2M = S2.dot( DirectOfFirstRotAxis ); // S2M = S" x M

				SSin1 = -S2D * S1M * MK + S1D * S2M * MK;
				SCos1 = S1D * S1M * MK + S2D * S2M * MK;

				// use atan2 to compute the slave angle
				SRotAngle1 = std::atan2( SSin1, SCos1 ) * IUtoBLU_Rotary;
				SRotAngle2 = SRotAngle1;
				return ALMID_MasterInfinityOfSolution;
			}
		}
		return ALMID_NoError;
	}

	int FiveAxisSolverCore::ChooseSolution(
		double MRotAngle1,
		double SRotAngle1,
		double MRotAngle2,
		double SRotAngle2,
		double LastMasterRotAngle,
		double LastSlaveRotAngle,
		double& MasterRotAngle,
		double& SlaveRotAngle,
		SolutionType type,
		double FStart,
		double FEnd,
		double SStart,
		double SEnd,
		int nRDOfFirst,
		int nRDOfSecond,
		double IUtoBLU_Rotary )
	{
		bool bPermissible1, bPermissible2;
		int nSolution = 0;
		double CachedMRotAng1 = MRotAngle1;
		double CachedMRotAng2 = MRotAngle2;
		double CachedSRotAng1 = SRotAngle1;
		double CachedSRotAng2 = SRotAngle2;

		// check if both the target position and the path (from last position to target position) of each solution are permissible.
		bPermissible1 = IsSolutionPermissible( MRotAngle1, SRotAngle1, LastMasterRotAngle, LastSlaveRotAngle, type,
			FStart, FEnd, SStart, SEnd, nRDOfFirst, nRDOfSecond, IUtoBLU_Rotary );
		bPermissible2 = IsSolutionPermissible( MRotAngle2, SRotAngle2, LastMasterRotAngle, LastSlaveRotAngle, type,
			FStart, FEnd, SStart, SEnd, nRDOfFirst, nRDOfSecond, IUtoBLU_Rotary );

		// both rotation angles of solution 1 are valid
		if( bPermissible1 == true ) {
			// both rotation angles of solution 2 are valid
			if( bPermissible2 == true ) {
				// choose solution consider master and slave distance meanwhile
				if( type == SolutionType::MSAngleShortestDist ) {
					double Sol1_TotalDis = std::sqrt( std::pow( MRotAngle1 - LastMasterRotAngle, 2 ) + std::pow( SRotAngle1 - LastSlaveRotAngle, 2 ) );
					double Sol2_TotalDis = std::sqrt( std::pow( MRotAngle2 - LastMasterRotAngle, 2 ) + std::pow( SRotAngle2 - LastSlaveRotAngle, 2 ) );

					// sol1 dis > sol2 dis at least 0.001
					if( Sol1_TotalDis - Sol2_TotalDis >= 0.001 ) {
						nSolution = 2;
					}
					else {
						nSolution = 1;
					}
				}
				// check the distance between 1st master root and last master is far smaller than another
				else if( std::fabs( MRotAngle1 - LastMasterRotAngle ) + FIVEAXIS_SOL_TOL < std::fabs( MRotAngle2 - LastMasterRotAngle ) ) {
					nSolution = 1;
				}
				else if( std::fabs( MRotAngle1 - LastMasterRotAngle ) > std::fabs( MRotAngle2 - LastMasterRotAngle ) + FIVEAXIS_SOL_TOL ) {
					nSolution = 2;
				}
				// check the distance between 1st slave root and last slave is far smaller than another
				else if( std::fabs( SRotAngle1 - LastSlaveRotAngle ) + FIVEAXIS_SOL_TOL < std::fabs( SRotAngle2 - LastSlaveRotAngle ) ) {
					nSolution = 1;
				}
				else if( std::fabs( SRotAngle1 - LastSlaveRotAngle ) > std::fabs( SRotAngle2 - LastSlaveRotAngle ) + FIVEAXIS_SOL_TOL ) {
					nSolution = 2;
				}
				// check the distance between 1st master root and last master is far smaller than another
				else if( std::fabs( CachedMRotAng1 ) + FIVEAXIS_SOL_TOL < std::fabs( CachedMRotAng2 ) ) {
					nSolution = 1;
				}
				else if( std::fabs( CachedMRotAng1 ) > std::fabs( CachedMRotAng2 ) + FIVEAXIS_SOL_TOL ) {
					nSolution = 2;
				}
				// check the distance between 1st slave root and last slave is far smaller than another
				else if( std::fabs( CachedSRotAng1 ) + FIVEAXIS_SOL_TOL < std::fabs( CachedSRotAng2 ) ) {
					nSolution = 1;
				}
				else if( std::fabs( CachedSRotAng1 ) > std::fabs( CachedSRotAng2 ) + FIVEAXIS_SOL_TOL ) {
					nSolution = 2;
				}
				// both move distances are smaller than solution tolerance, choose any one of the solutions
				else {
					nSolution = 1;
				}
			}
			// any rotation angle of solution 2 is invalid
			else {
				nSolution = 1;
			}
		}
		// any rotation angle of solution 1 is invalid
		else if( bPermissible2 == true ) {
			nSolution = 2;
		}
		// no valid solution, post alarm
		else {
			return ALMID_NoSolution;
		}

		if( nSolution == 1 ) {
			MasterRotAngle = MRotAngle1;
			SlaveRotAngle = SRotAngle1;
		}
		else {
			MasterRotAngle = MRotAngle2;
			SlaveRotAngle = SRotAngle2;
		}

		return ALMID_NoError;
	}
}
