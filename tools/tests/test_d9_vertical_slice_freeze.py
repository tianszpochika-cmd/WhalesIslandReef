import unittest

from tools.d9_vertical_slice_check import validate_freeze


class D9VerticalSliceFreezeTests(unittest.TestCase):
    def test_frozen_slice_is_complete_and_points_to_existing_d7_entries(self):
        self.assertEqual([], validate_freeze())


if __name__ == "__main__":
    unittest.main()
