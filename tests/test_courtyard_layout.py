"""Offline level-design checks against authored C# coordinates and pixel-font data.

These validate routes and text assets; Unity runtime behavior still needs a play test.
"""
from collections import deque
from pathlib import Path
import math
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
SOURCE = (ROOT / 'Assets/Scripts/Core/CourtyardGameplay.cs').read_text()
ART = (ROOT / 'Assets/Scripts/Core/ReutovRuntimeArt.cs').read_text()


def vectors(block):
    return [tuple(map(float, pair)) for pair in
            re.findall(r'new Vector2\((\d+), (\d+)\)', block)]


class CourtyardLayoutTests(unittest.TestCase):
    def test_all_five_objectives_can_be_reached_without_crossing_scenery(self):
        obstacles = [tuple(map(float, r)) for r in re.findall(
            r'new Rect\((\d+), (\d+), (\d+), (\d+)\)', SOURCE)]
        radius = float(re.search(r'const float Radius = ([\d.]+)f', SOURCE)[1])
        targets = vectors(SOURCE.split('Vector2[] targets = {')[1].split('};')[0])
        spawn = vectors(SOURCE.split('Vector2 spawn =')[1].split(';')[0])[0]
        self.assertEqual(len(targets), 5)
        self.assertEqual(len(obstacles), 8)

        def free(point):
            x, y = point
            if not (64 <= x <= 1215 and 240 <= y <= 661):
                return False
            for left, top, width, height in obstacles:
                dx = x - max(left, min(x, left + width))
                dy = y - max(top, min(y, top + height))
                if dx * dx + dy * dy < radius * radius:
                    return False
            return True

        self.assertTrue(free(spawn), 'spawn intersects scenery')
        # A fine four-connected grid checks continuous walking corridors, not teleportation.
        seen = {spawn}
        queue = deque([spawn])
        while queue:
            x, y = queue.popleft()
            for point in ((x-4, y), (x+4, y), (x, y-4), (x, y+4)):
                if point not in seen and free(point):
                    seen.add(point)
                    queue.append(point)
        for target in targets:
            with self.subTest(target=target):
                # Stricter than the 95px interaction range: player can stand very close.
                self.assertLess(min(math.dist(target, point) for point in seen), 40)

    def test_touch_controls_stay_inside_world_and_do_not_overlap(self):
        buttons = {}
        for name in ('Back', 'JoyBase', 'Run', 'Action', 'Light'):
            match = re.search(r'UI\("' + name + r'", new Vector2\(([-\d]+), ([-\d]+)\), new Vector2\((\d+), (\d+)\)', SOURCE)
            self.assertIsNotNone(match, name)
            x, y, width, height = map(float, match.groups())
            box = (x-width/2, y-height/2, x+width/2, y+height/2)
            self.assertTrue(-640 <= box[0] < box[2] <= 640, name)
            self.assertTrue(-360 <= box[1] < box[3] <= 360, name)
            for other, old in buttons.items():
                self.assertFalse(box[0] < old[2] and box[2] > old[0] and
                                 box[1] < old[3] and box[3] > old[1], (name, other))
            buttons[name] = box

    def test_story_lines_fit_panel_and_have_font_glyphs(self):
        glyphs = set(re.findall(r"Add\('(.)',", ART))
        story = SOURCE.split('conversations = new [] {')[1].split('};')[0]
        messages = re.findall(r'"([^"\n]*)"', story)
        self.assertEqual(len(messages), 9)
        for message in messages:
            for line in message.split(r'\n'):
                with self.subTest(line=line):
                    self.assertLessEqual(len(line) * 6 * 3, 1080)
                    self.assertFalse(set(line) - glyphs)
        for key in 'WASD SHIFT EF':
            self.assertIn(key, glyphs)


if __name__ == '__main__':
    unittest.main()
