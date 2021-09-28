#pragma once

#include "sl2.c"

__declspec(dllexport) void hash(unsigned char temp[], int n, unsigned long long* u)
{
	sl2_t id;
	sl2_unit(id); // initialize id

	// check empty array
	if (sizeof(temp) == 0 || n < 0)
	{
		n = 0;
	}
	//// get array size
	//int n = sizeof(temp) / sizeof(temp[0]);

	for (int i = 0; i < n; i++)
	{
		sl2_mul_bits_right(&id[0][0], &id[0][1], &id[1][0], &id[1][1], temp[i]);
	}

	unsigned long long* r = sl2_raw(id); // array of 8 unsigned long long
	memcpy(u, r, sizeof(unsigned long long)*8);
}
