---
name: speckit-common-ask-martin
description: 'Before asking Martin anything: decide whether to ask at all, then ask so he can answer from the message alone'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: common:commands/ask-martin.md
---

# Asking Martin

Martin decides; agents advise and deliver. Ask only what is his to decide, and make each ask one he can answer without scrolling back.

## Should you ask at all?

Ask only when one of these is true:

- the step is a hard limit: it spends money, creates a permanent name, touches someone else's work, pushes outside an approved pull request, or merges;
- the work is outside what he approved for this item;
- two sound options remain and no rule, note or recorded decision settles it.

First check the repo's rules, its notes file and the item. If one settles it, apply it and don't ask. A question of fact (where a file is, what a run did) is never put to him: find it out.

If only one option survives the check below, there is no question: take it, record it on the item with the options dropped and why, and say so in your report.

## Show it

He approves only what he has seen. Every ask says in plain words what the thing is, where it came from and why you are asking now, and shows it in the same message: the exact text in full, the diff or a link to it, or the file. Never "approve WP3" or "see above". Ask only about your own work, from your own session.

## The template

**QUESTION n: a plain title, about the product**
- **What:** what is being decided, in plain words.
- **Why:** the problem it answers, as someone using the product would see it.
- **How:** what would be built or changed, with the thing shown.
- **Options:** each answer, lettered, and what a person would see or be able to do with it.
- **Reasons:** for each option, why it is right or wrong, and the recommended one.
- **Impact:** for each option, what changes for the people using it, and what goes wrong if it is the wrong choice.
- **Reply:** the exact words for each option.

**Check every question before asking.** Write the question and every option, then have a separate reviewer check each option, seeing only the question, the options, the repo's architecture and core rules, the product outcome, and the item's Purpose and spec, never your reasoning. An option is valid only if it keeps the architecture and core rules, does the thing properly rather than fast (an option that cuts a corner to save time is invalid when a proper one exists), serves the product outcome, and delivers the item's Purpose and spec. With one valid option there is no question: take it, record it on the item with the dropped options and the test each failed, and say so in your report. With none, the question is wrong: rethink it. With two or more, ask Martin about those only, with one line naming each dropped option and the test it failed.

Number QUESTIONs upwards across the conversation and never reuse a number. One decision per block.

## His instruction is his approval

When Martin tells you to do something, that is his approval. Quote his words once where they apply (the item's `Approved:` or `Spec agreed:` line), act on them, and ask nothing more.

## After he answers

Record what he said, in his words, where it applies (the item, or the repo's notes file for a standing decision), then act on it in the same turn.
