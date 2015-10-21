# The Return-Value-Must-Not-Be-Ignored Attribute Analyser

Now that Visual Studio 2015 has made it incredibly easy\* for me to write code that checks whether *other* code that I write adheres to rules of my own imagining, it's time to see if I can put to bed an annoyance with using immutable collections.

\* (so they say)

Every now and then I make a stupid mistake like the following:

    // Why doesn't this write out "Item count: 3" ???!!!!!
    var list = ImmutableList.Of(1, 2);
    list.Add(3);
    Console.WriteLine("Item count: " + list.Count);
    
The "list" reference is of an immutable set type and so calling "Add" does not mutate the data that "list" points at (a good thing), but I've forgotten to keep hold of the new reference from that  "Add" returns - really, I meant to do this:

    // Ahh... three items now :)
    var list = ImmutableList.Of(1, 2);
    list = list.Add(3);
    Console.WriteLine("Item count: " + list.Count);
    
Until now, though, this has been the sort of silly mistake that the compiler wouldn't save you from - it can save you from a lot of things (like it won't let me add a string value to the list if the list only accepts ints), but not this.

The aim of this small project is to create an attribute which can be used to decorate functions whose return values should not be ignored..

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ReturnValueMustNotBeIgnoredAttribute : Attribute { }

.. and to create an Analyser which tells me that I've made that silly mistake again, catching it long before runtime.

![Analyser-in-action screenshot](http://www.productiverage.com/content/images/AssignReturnValueAnalyserScreenshot.png)

As of October 2015, this seems to be working in most of the simple cases I've tried. There are some rough edges that I may need to find better solutions for as I iron things out.

These should all be allowed:

    list = list.Add(3);
    ValidateList(list.Add(3));
    var isMatch = (otherList == list.Add(3));
    return list.Add(3);
    
This should not be:

    list.Add(3);