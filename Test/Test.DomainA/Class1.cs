﻿using Test.DomainB;

namespace Test.DomainA
{
    public class Class1
    {
        public static void MethodA()
        {
            var process = new DomainB.Process2();

            process.Method2(new Method2Input
            {
                Property0 = "a",
                Property1 = 1,
                Property2 = null,
                Property3 = 2,
                Property5 = new ()
                {
                    X="a",
                    Y = 6,
                    PropertyNestedUsage2 = new ()
                    {
                        Z = DateTime.MaxValue
                    }
                }
            });
        }
    }
}
