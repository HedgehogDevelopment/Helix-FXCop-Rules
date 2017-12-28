
<img src="https://www.hhog.com/-/media/PublicImages/Hedgehog/Hedgehog-logo-4color-275x46.jpg" alt="Hedgehog Development" border="0">

# Helix FxCop Rules
These FxCop rules are designed to catch when developers make incorrect cross-module dependency references according to [Helix principles](http://helix.sitecore.net/principles/architecture-principles/index.html).
By including these rules, developers no longer need to structure their solutions by separating modules into individual projects. Instead, modules may be separated by namespace, but could still be contained in to same project/DLL, and therefore not bloat the solution with increased build times.
Using these rules, developers can track down violations of the Helix design principals, and even prevent builds from succeeding when the principles are broken, ensuring safer longevity of their code.

# More Information
[FxCop Rules for Sitecore Helix Blog Post](https://www.hhog.com/blog/sitecore-helix-fxcop-rules)
