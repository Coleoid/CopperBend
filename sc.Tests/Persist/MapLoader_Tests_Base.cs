using CopperBend.Fabric;
using NUnit.Framework;
using CopperBend.Logic.Tests;
using CopperBend.Contract;

namespace CopperBend.Creation.Tests
{
    public class MapLoader_Tests_Base : Tests_Base
    {
        protected void Assert_Bridge_isRoundRoom(CompoundMapBridge bridge)
        {
            Assert_Bridges_equiv(bridge, Ser_Test_Data.Get_RR_Bridge());
        }

        protected void Assert_Bridges_equiv(CompoundMapBridge result, CompoundMapBridge expected)
        {
            Assert.That(result.Name, Is.EqualTo(expected.Name));
            Assert.That(result.Width, Is.EqualTo(expected.Width));
            Assert.That(result.Height, Is.EqualTo(expected.Height));

            // Checking values, not keys, since the legend keys may change
            foreach (var val in expected.Legend.Values)
            {
                Assert.That(result.Legend.ContainsValue(val), $"Legend value [{val}] missing");
            }
            Assert.That(result.Legend.Count, Is.EqualTo(expected.Legend.Count));

            for (int row = 0; row < expected.Terrain.Count; row++)
            {
                Assert.That(result.Terrain[row], Is.EqualTo(expected.Terrain[row]), $"Terrain row {row} didn't match");
            }
            Assert.That(result.Terrain.Count, Is.EqualTo(expected.Terrain.Count));

            for (int i = 0; i < expected.Triggers.Count; i++)
            {
                Assert_Trigger_equiv(result.Triggers[i], expected.Triggers[i]);
            }
        }

        protected void Assert_Trigger_equiv(Trigger result, Trigger expected)
        {
            Assert.That(result.Categories, Is.EqualTo(expected.Categories));
            Assert.That(result.Condition, Is.EqualTo(expected.Condition));
            Assert.That(result.Script, Is.EquivalentTo(expected.Script));
        }


        protected void Assert_Map_isRoundRoom(CompoundMap map)
        {
            Assert.That(map.Name, Is.EqualTo("Round Room"));
            Assert.That(map.Height, Is.EqualTo(5));
            Assert.That(map.Width, Is.EqualTo(7));
            Assert.That(map.SpaceMap.GetItem((1, 0)).Terrain.Name, Is.EqualTo("soil"));
            Assert.That(map.SpaceMap.GetItem((1, 1)).Terrain.Name, Is.EqualTo("stone wall"));
            Assert.That(map.SpaceMap.GetItem((5, 2)).Terrain.Name, Is.EqualTo("closed door"));
        }

        protected const string SaveFileYaml = @"!CompoundMapBridge
name: Tacker Farm
width: 46
height: 32
legend:
  ',': grass
  X: wooden fence
  .: soil
  '~': tilled soil
  =: wall
  +: closed door
  '>': stairs down
  '%': gate
  T: table
  w: tall weeds
terrain:
- ',,,,,XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX'
- ',,,,,X,,,....,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,X'
- ',,,,,X,,,,...,,,,,,,,,,,,,,,,,,,,~.~.~~~..~,,X'
- ',,,,,X.~~...~~~...~~~.,,,,.====,,~..~~.~~~.,,X'
- ',,,,,X...~~~.~~.~.~~..,,,,.=..=,,~~~~.~..~~,,X'
- ',,,,,X.~....~..~~~~~...,,,,=++=,,~.~.~~~~.~,,X'
- ',,,,,X.~~~.~~~~...~~~.,,,,,,..,,,..~~~..~~~,,X'
- ',,,,,X.~~.~~.~~~.~~.~.,,......,,,~.~.~.~.~~,,X'
- ',,,,,X.~~....~~~..~~~.....,,,,,,,,,,,,,,,,,,,X'
- ',,,,,X.~~~~~.~.~~.~~~.,,,,,,,,,,,==========,,X'
- ',,,,,X.~~..~~~.~~~~.~.,,,,,,,,,,,=.+.=....=,,X'
- ',,,,,X.~~~~~.~~...~.~.,,,,,,,,,,,=.=.+....=,,X'
- ',,,,,X.~~....~~..~~~~.,,,,,,,,,,,===.======,,X'
- ',,,,,X.~~.~~~..~~~~...,,,,.......=.......>=,,X'
- ',,,,,X.~~~..~~....~~~.,..........+........=,,X'
- '.....%......................,,,,.=..TT...T=,,X'
- '.....%.....................,,,,,.=.....TTT=,,X'
- '.....+................,,,...,,,,.==========,,X'
- ',,,,,X.~~~..~~~~~.~~~.,,,,..,,..,,,,,,,,,,,,,X'
- ',,,,,X.~~.~~~..~~~~.~......==..,,,,,,,,,,,,,,X'
- ',,,,,X.~~~...~~~..~~~.,,,,.==....,,,,,,,,,,,,X'
- ',,,,,X...~~...~~.~~~~.,,,,,,,,,X+XXXXXXXXXX,,X'
- ',,,,,X.~~...~~~~~~.~~.,,,,,,,,,X..........X,,X'
- ',,,,,X.~.~~~..~~~~..~.,,,,,,,,,X..........X,,X'
- ',,,,,X.~~~..~..~~~~...,,,ww,,,,X..........X,,X'
- ',,,,,X.~..~~~.~~~~.~~.,X%%%X,,,X..........X,,X'
- ',,,,,X...~~..~~~..~~~.,X,,,%,..+....X+XXXXX,,X'
- ',,,,,X.~~~..~~~.~~~.~.,X,,,Xw,,X....X.....X,,X'
- ',,,,,X.~~~~~~.~~..~~..,X,,,Xw,,X,,,,X.....X,,X'
- ',,,,,X................,XXXXXww,XXXXXXXXXXXX,,X'
- ',,,,,X,,,,,,,,,,,,,,,,,,,,wwww,,,,,,,,,,,,,,,X'
- ',,,,,XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX'
areaRots:
  (X=8, Y=0): !AreaRot
    name: Area rot
    iD: 1491
    health: 1
    maxHealth: 80
  (X=9, Y=0): !AreaRot
    name: Area rot
    iD: 1492
    health: 2
    maxHealth: 80
  (X=10, Y=0): !AreaRot
    name: Area rot
    iD: 1493
    health: 2
    maxHealth: 80
  (X=11, Y=0): !AreaRot
    name: Area rot
    iD: 1494
    health: 1
    maxHealth: 80
  (X=9, Y=1): !AreaRot
    name: Area rot
    iD: 1495
    health: 1
    maxHealth: 80
  (X=10, Y=1): !AreaRot
    name: Area rot
    iD: 1496
    health: 2
    maxHealth: 80
  (X=11, Y=1): !AreaRot
    name: Area rot
    iD: 1497
    health: 1
    maxHealth: 80
  (X=12, Y=1): !AreaRot
    name: Area rot
    iD: 1498
    health: 1
    maxHealth: 80
  (X=10, Y=2): !AreaRot
    name: Area rot
    iD: 1499
    health: 1
    maxHealth: 80
  (X=11, Y=2): !AreaRot
    name: Area rot
    iD: 1500
    health: 2
    maxHealth: 80
  (X=12, Y=2): !AreaRot
    name: Area rot
    iD: 1501
    health: 1
    maxHealth: 80
  (X=7, Y=3): !AreaRot
    name: Area rot
    iD: 1502
    health: 1
    maxHealth: 80
  (X=8, Y=3): !AreaRot
    name: Area rot
    iD: 1503
    health: 1
    maxHealth: 80
  (X=9, Y=3): !AreaRot
    name: Area rot
    iD: 1504
    health: 1
    maxHealth: 80
  (X=10, Y=3): !AreaRot
    name: Area rot
    iD: 1505
    health: 2
    maxHealth: 80
  (X=11, Y=3): !AreaRot
    name: Area rot
    iD: 1506
    health: 2
    maxHealth: 80
  (X=12, Y=3): !AreaRot
    name: Area rot
    iD: 1507
    health: 1
    maxHealth: 80
  (X=13, Y=3): !AreaRot
    name: Area rot
    iD: 1508
    health: 1
    maxHealth: 80
  (X=6, Y=4): !AreaRot
    name: Area rot
    iD: 1509
    health: 1
    maxHealth: 80
  (X=7, Y=4): !AreaRot
    name: Area rot
    iD: 1510
    health: 1
    maxHealth: 80
  (X=8, Y=4): !AreaRot
    name: Area rot
    iD: 1511
    health: 1
    maxHealth: 80
  (X=9, Y=4): !AreaRot
    name: Area rot
    iD: 1512
    health: 2
    maxHealth: 80
  (X=10, Y=4): !AreaRot
    name: Area rot
    iD: 1513
    health: 2
    maxHealth: 80
  (X=11, Y=4): !AreaRot
    name: Area rot
    iD: 1514
    health: 2
    maxHealth: 80
  (X=12, Y=4): !AreaRot
    name: Area rot
    iD: 1515
    health: 2
    maxHealth: 80
  (X=13, Y=4): !AreaRot
    name: Area rot
    iD: 1516
    health: 1
    maxHealth: 80
  (X=14, Y=4): !AreaRot
    name: Area rot
    iD: 1517
    health: 1
    maxHealth: 80
  (X=15, Y=4): !AreaRot
    name: Area rot
    iD: 1518
    health: 1
    maxHealth: 80
  (X=16, Y=4): !AreaRot
    name: Area rot
    iD: 1519
    health: 1
    maxHealth: 80
  (X=20, Y=4): !AreaRot
    name: Area rot
    iD: 1520
    health: 1
    maxHealth: 80
  (X=21, Y=4): !AreaRot
    name: Area rot
    iD: 1521
    health: 1
    maxHealth: 80
  (X=7, Y=5): !AreaRot
    name: Area rot
    iD: 1522
    health: 1
    maxHealth: 80
  (X=8, Y=5): !AreaRot
    name: Area rot
    iD: 1523
    health: 1
    maxHealth: 80
  (X=9, Y=5): !AreaRot
    name: Area rot
    iD: 1524
    health: 1
    maxHealth: 80
  (X=10, Y=5): !AreaRot
    name: Area rot
    iD: 1525
    health: 1
    maxHealth: 80
  (X=11, Y=5): !AreaRot
    name: Area rot
    iD: 1526
    health: 2
    maxHealth: 80
  (X=12, Y=5): !AreaRot
    name: Area rot
    iD: 1527
    health: 3
    maxHealth: 80
  (X=13, Y=5): !AreaRot
    name: Area rot
    iD: 1528
    health: 2
    maxHealth: 80
  (X=14, Y=5): !AreaRot
    name: Area rot
    iD: 1529
    health: 2
    maxHealth: 80
  (X=15, Y=5): !AreaRot
    name: Area rot
    iD: 1530
    health: 2
    maxHealth: 80
  (X=16, Y=5): !AreaRot
    name: Area rot
    iD: 1531
    health: 1
    maxHealth: 80
  (X=17, Y=5): !AreaRot
    name: Area rot
    iD: 1532
    health: 1
    maxHealth: 80
  (X=18, Y=5): !AreaRot
    name: Area rot
    iD: 1533
    health: 1
    maxHealth: 80
  (X=19, Y=5): !AreaRot
    name: Area rot
    iD: 1534
    health: 1
    maxHealth: 80
  (X=20, Y=5): !AreaRot
    name: Area rot
    iD: 1535
    health: 1
    maxHealth: 80
  (X=21, Y=5): !AreaRot
    name: Area rot
    iD: 1536
    health: 1
    maxHealth: 80
  (X=22, Y=5): !AreaRot
    name: Area rot
    iD: 1537
    health: 1
    maxHealth: 80
  (X=7, Y=6): !AreaRot
    name: Area rot
    iD: 1538
    health: 1
    maxHealth: 80
  (X=8, Y=6): !AreaRot
    name: Area rot
    iD: 1539
    health: 1
    maxHealth: 80
  (X=9, Y=6): !AreaRot
    name: Area rot
    iD: 1540
    health: 1
    maxHealth: 80
  (X=10, Y=6): !AreaRot
    name: Area rot
    iD: 1541
    health: 1
    maxHealth: 80
  (X=11, Y=6): !AreaRot
    name: Area rot
    iD: 1542
    health: 3
    maxHealth: 80
  (X=12, Y=6): !AreaRot
    name: Area rot
    iD: 1543
    health: 3
    maxHealth: 80
  (X=13, Y=6): !AreaRot
    name: Area rot
    iD: 1544
    health: 3
    maxHealth: 80
  (X=14, Y=6): !AreaRot
    name: Area rot
    iD: 1545
    health: 2
    maxHealth: 80
  (X=15, Y=6): !AreaRot
    name: Area rot
    iD: 1546
    health: 2
    maxHealth: 80
  (X=16, Y=6): !AreaRot
    name: Area rot
    iD: 1547
    health: 2
    maxHealth: 80
  (X=17, Y=6): !AreaRot
    name: Area rot
    iD: 1548
    health: 1
    maxHealth: 80
  (X=18, Y=6): !AreaRot
    name: Area rot
    iD: 1549
    health: 1
    maxHealth: 80
  (X=19, Y=6): !AreaRot
    name: Area rot
    iD: 1550
    health: 1
    maxHealth: 80
  (X=20, Y=6): !AreaRot
    name: Area rot
    iD: 1551
    health: 1
    maxHealth: 80
  (X=6, Y=7): !AreaRot
    name: Area rot
    iD: 1552
    health: 1
    maxHealth: 80
  (X=7, Y=7): !AreaRot
    name: Area rot
    iD: 1553
    health: 1
    maxHealth: 80
  (X=8, Y=7): !AreaRot
    name: Area rot
    iD: 1554
    health: 1
    maxHealth: 80
  (X=9, Y=7): !AreaRot
    name: Area rot
    iD: 1555
    health: 1
    maxHealth: 80
  (X=10, Y=7): !AreaRot
    name: Area rot
    iD: 1556
    health: 1
    maxHealth: 80
  (X=11, Y=7): !AreaRot
    name: Area rot
    iD: 1557
    health: 1
    maxHealth: 80
  (X=12, Y=7): !AreaRot
    name: Area rot
    iD: 1558
    health: 3
    maxHealth: 80
  (X=13, Y=7): !AreaRot
    name: Area rot
    iD: 1559
    health: 3
    maxHealth: 80
  (X=14, Y=7): !AreaRot
    name: Area rot
    iD: 1560
    health: 3
    maxHealth: 80
  (X=15, Y=7): !AreaRot
    name: Area rot
    iD: 1561
    health: 2
    maxHealth: 80
  (X=16, Y=7): !AreaRot
    name: Area rot
    iD: 1562
    health: 2
    maxHealth: 80
  (X=17, Y=7): !AreaRot
    name: Area rot
    iD: 1563
    health: 1
    maxHealth: 80
  (X=18, Y=7): !AreaRot
    name: Area rot
    iD: 1564
    health: 1
    maxHealth: 80
  (X=19, Y=7): !AreaRot
    name: Area rot
    iD: 1565
    health: 1
    maxHealth: 80
  (X=20, Y=7): !AreaRot
    name: Area rot
    iD: 1566
    health: 1
    maxHealth: 80
  (X=6, Y=8): !AreaRot
    name: Area rot
    iD: 1567
    health: 1
    maxHealth: 80
  (X=7, Y=8): !AreaRot
    name: Area rot
    iD: 1568
    health: 1
    maxHealth: 80
  (X=8, Y=8): !AreaRot
    name: Area rot
    iD: 1569
    health: 1
    maxHealth: 80
  (X=9, Y=8): !AreaRot
    name: Area rot
    iD: 1570
    health: 1
    maxHealth: 80
  (X=10, Y=8): !AreaRot
    name: Area rot
    iD: 1571
    health: 1
    maxHealth: 80
  (X=11, Y=8): !AreaRot
    name: Area rot
    iD: 1572
    health: 3
    maxHealth: 80
  (X=12, Y=8): !AreaRot
    name: Area rot
    iD: 1573
    health: 3
    maxHealth: 80
  (X=13, Y=8): !AreaRot
    name: Area rot
    iD: 1574
    health: 3
    maxHealth: 80
  (X=14, Y=8): !AreaRot
    name: Area rot
    iD: 1575
    health: 2
    maxHealth: 80
  (X=15, Y=8): !AreaRot
    name: Area rot
    iD: 1576
    health: 2
    maxHealth: 80
  (X=16, Y=8): !AreaRot
    name: Area rot
    iD: 1577
    health: 1
    maxHealth: 80
  (X=17, Y=8): !AreaRot
    name: Area rot
    iD: 1578
    health: 1
    maxHealth: 80
  (X=18, Y=8): !AreaRot
    name: Area rot
    iD: 1579
    health: 1
    maxHealth: 80
  (X=19, Y=8): !AreaRot
    name: Area rot
    iD: 1580
    health: 1
    maxHealth: 80
  (X=7, Y=9): !AreaRot
    name: Area rot
    iD: 1581
    health: 1
    maxHealth: 80
  (X=8, Y=9): !AreaRot
    name: Area rot
    iD: 1582
    health: 1
    maxHealth: 80
  (X=9, Y=9): !AreaRot
    name: Area rot
    iD: 1583
    health: 1
    maxHealth: 80
  (X=10, Y=9): !AreaRot
    name: Area rot
    iD: 1584
    health: 1
    maxHealth: 80
  (X=11, Y=9): !AreaRot
    name: Area rot
    iD: 1585
    health: 3
    maxHealth: 80
  (X=12, Y=9): !AreaRot
    name: Area rot
    iD: 1586
    health: 3
    maxHealth: 80
  (X=13, Y=9): !AreaRot
    name: Area rot
    iD: 1587
    health: 3
    maxHealth: 80
  (X=14, Y=9): !AreaRot
    name: Area rot
    iD: 1588
    health: 1
    maxHealth: 80
  (X=15, Y=9): !AreaRot
    name: Area rot
    iD: 1589
    health: 1
    maxHealth: 80
  (X=16, Y=9): !AreaRot
    name: Area rot
    iD: 1590
    health: 1
    maxHealth: 80
  (X=18, Y=9): !AreaRot
    name: Area rot
    iD: 1591
    health: 1
    maxHealth: 80
  (X=19, Y=9): !AreaRot
    name: Area rot
    iD: 1592
    health: 1
    maxHealth: 80
  (X=7, Y=10): !AreaRot
    name: Area rot
    iD: 1593
    health: 1
    maxHealth: 80
  (X=8, Y=10): !AreaRot
    name: Area rot
    iD: 1594
    health: 1
    maxHealth: 80
  (X=9, Y=10): !AreaRot
    name: Area rot
    iD: 1595
    health: 3
    maxHealth: 80
  (X=10, Y=10): !AreaRot
    name: Area rot
    iD: 1596
    health: 3
    maxHealth: 80
  (X=11, Y=10): !AreaRot
    name: Area rot
    iD: 1597
    health: 2
    maxHealth: 80
  (X=12, Y=10): !AreaRot
    name: Area rot
    iD: 1598
    health: 2
    maxHealth: 80
  (X=13, Y=10): !AreaRot
    name: Area rot
    iD: 1599
    health: 1
    maxHealth: 80
  (X=14, Y=10): !AreaRot
    name: Area rot
    iD: 1600
    health: 1
    maxHealth: 80
  (X=15, Y=10): !AreaRot
    name: Area rot
    iD: 1601
    health: 1
    maxHealth: 80
  (X=16, Y=10): !AreaRot
    name: Area rot
    iD: 1602
    health: 1
    maxHealth: 80
  (X=18, Y=10): !AreaRot
    name: Area rot
    iD: 1603
    health: 1
    maxHealth: 80
  (X=19, Y=10): !AreaRot
    name: Area rot
    iD: 1604
    health: 1
    maxHealth: 80
  (X=7, Y=11): !AreaRot
    name: Area rot
    iD: 1605
    health: 1
    maxHealth: 80
  (X=8, Y=11): !AreaRot
    name: Area rot
    iD: 1606
    health: 1
    maxHealth: 80
  (X=9, Y=11): !AreaRot
    name: Area rot
    iD: 1607
    health: 2
    maxHealth: 80
  (X=10, Y=11): !AreaRot
    name: Area rot
    iD: 1608
    health: 3
    maxHealth: 80
  (X=11, Y=11): !AreaRot
    name: Area rot
    iD: 1609
    health: 2
    maxHealth: 80
  (X=12, Y=11): !AreaRot
    name: Area rot
    iD: 1610
    health: 3
    maxHealth: 80
  (X=13, Y=11): !AreaRot
    name: Area rot
    iD: 1611
    health: 2
    maxHealth: 80
  (X=14, Y=11): !AreaRot
    name: Area rot
    iD: 1612
    health: 1
    maxHealth: 80
  (X=15, Y=11): !AreaRot
    name: Area rot
    iD: 1613
    health: 1
    maxHealth: 80
  (X=16, Y=11): !AreaRot
    name: Area rot
    iD: 1614
    health: 1
    maxHealth: 80
  (X=7, Y=12): !AreaRot
    name: Area rot
    iD: 1615
    health: 1
    maxHealth: 80
  (X=8, Y=12): !AreaRot
    name: Area rot
    iD: 1616
    health: 1
    maxHealth: 80
  (X=9, Y=12): !AreaRot
    name: Area rot
    iD: 1617
    health: 2
    maxHealth: 80
  (X=10, Y=12): !AreaRot
    name: Area rot
    iD: 1618
    health: 1
    maxHealth: 80
  (X=11, Y=12): !AreaRot
    name: Area rot
    iD: 1619
    health: 1
    maxHealth: 80
  (X=12, Y=12): !AreaRot
    name: Area rot
    iD: 1620
    health: 1
    maxHealth: 80
  (X=13, Y=12): !AreaRot
    name: Area rot
    iD: 1621
    health: 3
    maxHealth: 80
  (X=14, Y=12): !AreaRot
    name: Area rot
    iD: 1622
    health: 3
    maxHealth: 80
  (X=15, Y=12): !AreaRot
    name: Area rot
    iD: 1623
    health: 1
    maxHealth: 80
  (X=16, Y=12): !AreaRot
    name: Area rot
    iD: 1624
    health: 1
    maxHealth: 80
  (X=17, Y=12): !AreaRot
    name: Area rot
    iD: 1625
    health: 1
    maxHealth: 80
  (X=18, Y=12): !AreaRot
    name: Area rot
    iD: 1626
    health: 1
    maxHealth: 80
  (X=7, Y=13): !AreaRot
    name: Area rot
    iD: 1627
    health: 1
    maxHealth: 80
  (X=8, Y=13): !AreaRot
    name: Area rot
    iD: 1628
    health: 1
    maxHealth: 80
  (X=9, Y=13): !AreaRot
    name: Area rot
    iD: 1629
    health: 2
    maxHealth: 80
  (X=10, Y=13): !AreaRot
    name: Area rot
    iD: 1630
    health: 1
    maxHealth: 80
  (X=11, Y=13): !AreaRot
    name: Area rot
    iD: 1631
    health: 1
    maxHealth: 80
  (X=12, Y=13): !AreaRot
    name: Area rot
    iD: 1632
    health: 1
    maxHealth: 80
  (X=13, Y=13): !AreaRot
    name: Area rot
    iD: 1633
    health: 1
    maxHealth: 80
  (X=14, Y=13): !AreaRot
    name: Area rot
    iD: 1634
    health: 2
    maxHealth: 80
  (X=15, Y=13): !AreaRot
    name: Area rot
    iD: 1635
    health: 2
    maxHealth: 80
  (X=16, Y=13): !AreaRot
    name: Area rot
    iD: 1636
    health: 1
    maxHealth: 80
  (X=17, Y=13): !AreaRot
    name: Area rot
    iD: 1637
    health: 1
    maxHealth: 80
  (X=18, Y=13): !AreaRot
    name: Area rot
    iD: 1638
    health: 1
    maxHealth: 80
  (X=7, Y=14): !AreaRot
    name: Area rot
    iD: 1639
    health: 1
    maxHealth: 80
  (X=8, Y=14): !AreaRot
    name: Area rot
    iD: 1640
    health: 1
    maxHealth: 80
  (X=9, Y=14): !AreaRot
    name: Area rot
    iD: 1641
    health: 1
    maxHealth: 80
  (X=10, Y=14): !AreaRot
    name: Area rot
    iD: 1642
    health: 1
    maxHealth: 80
  (X=12, Y=14): !AreaRot
    name: Area rot
    iD: 1643
    health: 1
    maxHealth: 80
  (X=13, Y=14): !AreaRot
    name: Area rot
    iD: 1644
    health: 1
    maxHealth: 80
  (X=14, Y=14): !AreaRot
    name: Area rot
    iD: 1645
    health: 1
    maxHealth: 80
  (X=15, Y=14): !AreaRot
    name: Area rot
    iD: 1646
    health: 1
    maxHealth: 80
  (X=16, Y=14): !AreaRot
    name: Area rot
    iD: 1647
    health: 1
    maxHealth: 80
  (X=17, Y=14): !AreaRot
    name: Area rot
    iD: 1648
    health: 1
    maxHealth: 80
  (X=8, Y=15): !AreaRot
    name: Area rot
    iD: 1649
    health: 1
    maxHealth: 80
  (X=17, Y=15): !AreaRot
    name: Area rot
    iD: 1650
    health: 1
    maxHealth: 80
  (X=8, Y=16): !AreaRot
    name: Area rot
    iD: 1651
    health: 1
    maxHealth: 80
  (X=9, Y=16): !AreaRot
    name: Area rot
    iD: 1652
    health: 1
    maxHealth: 80
  (X=16, Y=16): !AreaRot
    name: Area rot
    iD: 1653
    health: 1
    maxHealth: 80
  (X=17, Y=16): !AreaRot
    name: Area rot
    iD: 1654
    health: 1
    maxHealth: 80
  (X=8, Y=17): !AreaRot
    name: Area rot
    iD: 1655
    health: 1
    maxHealth: 80
  (X=9, Y=17): !AreaRot
    name: Area rot
    iD: 1656
    health: 1
    maxHealth: 80
  (X=16, Y=17): !AreaRot
    name: Area rot
    iD: 1657
    health: 1
    maxHealth: 80
  (X=16, Y=18): !AreaRot
    name: Area rot
    iD: 1658
    health: 1
    maxHealth: 80
  (X=17, Y=18): !AreaRot
    name: Area rot
    iD: 1659
    health: 1
    maxHealth: 80
  (X=18, Y=18): !AreaRot
    name: Area rot
    iD: 1660
    health: 1
    maxHealth: 80
  (X=17, Y=19): !AreaRot
    name: Area rot
    iD: 1661
    health: 1
    maxHealth: 80
  (X=18, Y=19): !AreaRot
    name: Area rot
    iD: 1662
    health: 1
    maxHealth: 80
  (X=19, Y=19): !AreaRot
    name: Area rot
    iD: 1663
    health: 1
    maxHealth: 80
  (X=16, Y=20): !AreaRot
    name: Area rot
    iD: 1664
    health: 1
    maxHealth: 80
  (X=17, Y=20): !AreaRot
    name: Area rot
    iD: 1665
    health: 1
    maxHealth: 80
  (X=11, Y=21): !AreaRot
    name: Area rot
    iD: 1666
    health: 1
    maxHealth: 80
  (X=16, Y=21): !AreaRot
    name: Area rot
    iD: 1667
    health: 1
    maxHealth: 80
  (X=9, Y=22): !AreaRot
    name: Area rot
    iD: 1668
    health: 1
    maxHealth: 80
  (X=10, Y=22): !AreaRot
    name: Area rot
    iD: 1669
    health: 1
    maxHealth: 80
  (X=11, Y=22): !AreaRot
    name: Area rot
    iD: 1670
    health: 1
    maxHealth: 80
  (X=12, Y=22): !AreaRot
    name: Area rot
    iD: 1671
    health: 1
    maxHealth: 80
  (X=15, Y=22): !AreaRot
    name: Area rot
    iD: 1672
    health: 1
    maxHealth: 80
  (X=16, Y=22): !AreaRot
    name: Area rot
    iD: 1673
    health: 1
    maxHealth: 80
  (X=10, Y=23): !AreaRot
    name: Area rot
    iD: 1674
    health: 1
    maxHealth: 80
  (X=11, Y=23): !AreaRot
    name: Area rot
    iD: 1675
    health: 1
    maxHealth: 80
  (X=12, Y=23): !AreaRot
    name: Area rot
    iD: 1676
    health: 1
    maxHealth: 80
  (X=13, Y=23): !AreaRot
    name: Area rot
    iD: 1677
    health: 1
    maxHealth: 80
  (X=14, Y=23): !AreaRot
    name: Area rot
    iD: 1678
    health: 1
    maxHealth: 80
  (X=15, Y=23): !AreaRot
    name: Area rot
    iD: 1679
    health: 1
    maxHealth: 80
  (X=9, Y=24): !AreaRot
    name: Area rot
    iD: 1680
    health: 1
    maxHealth: 80
  (X=10, Y=24): !AreaRot
    name: Area rot
    iD: 1681
    health: 1
    maxHealth: 80
  (X=11, Y=24): !AreaRot
    name: Area rot
    iD: 1682
    health: 2
    maxHealth: 80
  (X=12, Y=24): !AreaRot
    name: Area rot
    iD: 1683
    health: 3
    maxHealth: 80
  (X=13, Y=24): !AreaRot
    name: Area rot
    iD: 1684
    health: 2
    maxHealth: 80
  (X=14, Y=24): !AreaRot
    name: Area rot
    iD: 1685
    health: 1
    maxHealth: 80
  (X=15, Y=24): !AreaRot
    name: Area rot
    iD: 1686
    health: 1
    maxHealth: 80
  (X=7, Y=25): !AreaRot
    name: Area rot
    iD: 1687
    health: 1
    maxHealth: 80
  (X=8, Y=25): !AreaRot
    name: Area rot
    iD: 1688
    health: 1
    maxHealth: 80
  (X=9, Y=25): !AreaRot
    name: Area rot
    iD: 1689
    health: 1
    maxHealth: 80
  (X=12, Y=25): !AreaRot
    name: Area rot
    iD: 1690
    health: 1
    maxHealth: 80
  (X=13, Y=25): !AreaRot
    name: Area rot
    iD: 1691
    health: 1
    maxHealth: 80
  (X=14, Y=25): !AreaRot
    name: Area rot
    iD: 1692
    health: 1
    maxHealth: 80
  (X=13, Y=26): !AreaRot
    name: Area rot
    iD: 1693
    health: 1
    maxHealth: 80
multiBeings:
  0: !Coord_IBeing
    coord: (X=23, Y=20)
    being: !Being
      beingType: Being
      health: 20
      maxHealth: 20
      energy: 140
      maxEnergy: 140
      foreground:
        b: 34
        g: 139
        r: 34
        a: 255
        packedValue: 4280453922
      background:
        b: 0
        g: 0
        r: 0
        a: 255
        packedValue: 4278190080
      glyph: 64
      awareness: 6
      name: Suvail
      stratCat: UserInput
      isPlayer: true
      inventoryPersistenceList:
      - !Item
        iD: 1
        itemType: hoe
        aspects: {}
        attackMethod: !AttackMethod
          attackEffects:
          - !AttackEffect
            type: physical.impact.blunt
            damageRange: 1d4
        foreground: &o0
          b: 0
          g: 0
          r: 0
          a: 0
          packedValue: 0
        glyph: 0
        name: hoe
        quantity: 1
      - !Item
        iD: 2
        itemType: Item
        aspects: {}
        attackMethod: !AttackMethod
          attackEffects:
          - !AttackEffect
            type: physical.impact.blunt
            damageRange: 1d4
        foreground: *o0
        glyph: 0
        name: seed
        quantity: 1
      iD: 0
multiItems:
  1489:
    coord: (X=28, Y=4)
    item: !Item
      iD: 1489
      itemType: hoe
      aspects: {}
      attackMethod: !AttackMethod
        attackEffects:
        - !AttackEffect
          type: physical.impact.blunt
          damageRange: 1d4
      foreground: *o0
      glyph: 0
      name: hoe
      quantity: 1
  1490:
    coord: (X=28, Y=4)
    item: !Item
      iD: 1490
      itemType: gloves
      aspects: {}
      attackMethod: !AttackMethod
        attackEffects:
        - !AttackEffect
          type: physical.impact.blunt
          damageRange: 1d4
      foreground: *o0
      glyph: 0
      name: gloves
      quantity: 1
triggers:
- !Trigger
  name: wake up
  categories: MapChanged
  condition: (0, 0) to (99, 99)
  script:
  - move player (20, 20)
  - message
  - I wake up.  Cold--frost on the ground, except where I was lying.
  - Everything hurts when I stand up.
  - The sky... says it's morning.  A small farmhouse to the east.
  - Something real wrong with the ground to the west, and the northwest.
  - end message
  - remove trigger
- !Trigger
  name: Not Ready to Leave
  categories: PlayerLocation
  condition: (5, 16) to (5, 18)
  script:
  - message
  - Something needs doing here, first.
  - end message
  - 'move player: (6, 17)'
";
    }
}
