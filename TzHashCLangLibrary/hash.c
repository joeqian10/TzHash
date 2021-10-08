#include "string.h"

#include "sl2.c"

#ifdef _MSC_VER
__declspec(dllexport) void hash(unsigned char temp[], int n, unsigned long long* u)
{
	sl2_t id;
	sl2_unit(id); // initialize id

	// check empty array
	if (temp == NULL || n < 0)
	{
		n = 0;
	}

	for (int i = 0; i < n; i++)
	{
		sl2_mul_bits_right(&id[0][0], &id[0][1], &id[1][0], &id[1][1], temp[i]);
	}
	
	unsigned long long* r = sl2_raw(id); // array of 8 unsigned long long
	memcpy(u, r, sizeof(unsigned long long) * 8);
}
#endif // _MSC_VER

#ifdef __GNUC__
/// <summary>
/// 
/// </summary>
/// <param name="temp">is a pointer</param>
/// <param name="n"></param>
/// <param name="u"></param>
void hash(unsigned char temp[], int n, unsigned long long* u)
{
	sl2_t id;
	sl2_unit(id); // initialize id

	// check empty array
	if (temp == NULL || n < 0)
	{
		n = 0;
	}

	for (int i = 0; i < n; i++)
	{
		sl2_mul_bits_right(&id[0][0], &id[0][1], &id[1][0], &id[1][1], temp[i]);
	}

	//unsigned long long* r = sl2_raw(id); // array of 8 unsigned long long
	//memcpy(u, r, sizeof(unsigned long long) * 8);

	unsigned long long t[8] = { 0, 0, 0, 0, 0, 0, 0, 0 };
	sl2_raw_linux(id, t); // array of 8 unsigned long long
	memcpy(u, t, sizeof(unsigned long long) * 8);
}
#endif // __GNUC__
