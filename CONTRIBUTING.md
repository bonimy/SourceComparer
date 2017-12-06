# Contributing

## Welcome!
If you've taken an interest in this project, then hooray for our shared interest in physics. Please read this document to better understand what kinds of contributions are helpful to us.

## Table of Contents
* [How to file a bug report](#how-to-file-a-bug-report)
* [How to suggest a new feature](#how-to-suggest-a-new-feature)
* [How to set up your environment and run tests](#how-to-set-up-your-environment-and-run-tests)
* [The types of contributions we're looking for](#the-types-of-contributions-were-looking-for)
* [Our vision for the project](#our-vision-for-the-project)
* [Code styles](#code-styles)
    - [Commit messages](#commit-messages)

## How to file a bug report
To file a bug report, visit the [Issues](https://github.com/catwise/SourceComparer/issues) page, and search if the bug has already been reported. If it has not, then open a new issue, giving a short, descriptive title explaining the bug. From there, you will get a template file that outlines how to describe your bug and what information we are looking for when you submit it.

## How to suggest a new feature
Same as filing a bug report, open a new [Issue](https://github.com/catwise/SourceComparer/issues) and follow the guidelines in the template.

## How to set up your environment and run tests
> Presently, the [Visual Studio 2017 IDE](https://www.visualstudio.com/en-us/news/releasenotes/vs2017-relnotes) is the only supported environment. Users are encouraged to suggest new environments in our [Issues](https://github.com/catwise/SourceComparer/issues) section.

## The types of contributions we're looking for
The following contributions are greatly supported:
* Unit/Benchmark tests
* Source code documentation
* Bug fixes
* Cross-platform support
* Cross-IDE support
* Issue closers

Below are some things we think wouldn't be helpful for contributions
* Saying "I can't build on my computer" without providing details on what you tried, citing the README and the CONTRIBUTING file.
* Submitting a pull request with no description of what the code does. Even if it's a typo fix, mention that it's a typo fix.
* Submitting a pull request without opening an Issue first. Every pull request should be a solution to an existing issue. For very trivial things like typos, this is an exception. However, one line bug fix patches should still have an issue open to address the bug.
* Submitting a bug fix without an accompanying unit test to catch the bug from now on (except in cases of bugs that were typos e.g. `if (x = true)` to `if (x == true)`.
* Requesting a feature that is far outside the scope of this project.

## Our vision for the project
This is a simple project that pulls RA/Dec sources from source lists and does a bunch of stat stuff to them. Honestly, its scope extends to whatever I need it to do. I'd like to keep it simple but it's so easy to make a simple project become big.

## Code styles
Refer to our [CODE STYLES](CODE_STYLES.md) guide for an in-depth discussion on how to write your code for this project.

### Commit messages
[This](https://chris.beams.io/posts/git-commit/) article does a great job describing the value of a formatted git commit message. The things to take away from it are
1. [Separate subject from body with a blank line](https://chris.beams.io/posts/git-commit/#separate)
2. [Limit the subject line to 50 characters](https://chris.beams.io/posts/git-commit/#limit-50)
3. [Capitalize the subject line](https://chris.beams.io/posts/git-commit/#capitalize)
4. [Do not end the subject line with a period](https://chris.beams.io/posts/git-commit/#end)
5. [Use the imperative mood in the subject line](https://chris.beams.io/posts/git-commit/#imperative)
6. [Wrap the body at 72 characters](https://chris.beams.io/posts/git-commit/#wrap-72)
7. [Use the body to explain _what_ and _why_ vs. _how_](https://chris.beams.io/posts/git-commit/#why-not-how)

Try to comply to these rules as much as possible.
